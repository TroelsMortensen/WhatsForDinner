import json
import math
import re
import shutil
import unicodedata
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from statistics import median
from typing import Any

from rapidocr_onnxruntime import RapidOCR


ROOT = Path(r"c:\Users\trmo\RiderProjects\WhatsForDinner")
MANIFEST = ROOT / "RawDataExtracted" / "image-paths.txt"
RECIPES_DIR = ROOT / "UI" / "wwwroot" / "recipes"
IMAGES_DIR = ROOT / "UI" / "wwwroot" / "images" / "recipe-images"
INDEX_PATH = RECIPES_DIR / "index.json"
REVIEW_PATH = ROOT / "RawDataExtracted" / "mapping-review.json"


def read_manifest_lines(path: Path) -> list[str]:
    candidates = ["utf-16", "utf-8-sig", "utf-8", "cp1252", "latin-1"]
    for enc in candidates:
        try:
            text = path.read_text(encoding=enc)
            lines = [line.strip() for line in text.splitlines() if line.strip()]
            if lines:
                return lines
        except Exception:
            continue
    raise RuntimeError(f"Unable to decode manifest file: {path}")


def normalize_for_match(text: str) -> str:
    stripped = unicodedata.normalize("NFKD", text)
    ascii_only = "".join(ch for ch in stripped if not unicodedata.combining(ch))
    return re.sub(r"\s+", " ", ascii_only).strip().lower()


def sanitize_filename(text: str) -> str:
    text = re.sub(r'[<>:"/\\|?*]', "", text).strip()
    text = re.sub(r"\s+", " ", text)
    return text


@dataclass
class OcrLine:
    text: str
    x: float
    y: float


@dataclass
class OcrImage:
    path: Path
    lines: list[OcrLine]
    text_joined: str
    keyword_score: int
    char_count: int


def run_ocr(engine: RapidOCR, image_path: Path) -> OcrImage:
    result, _ = engine(str(image_path))
    lines: list[OcrLine] = []
    joined_parts: list[str] = []
    if result:
        for item in result:
            box, text, _ = item
            x = min(point[0] for point in box)
            y = min(point[1] for point in box)
            cleaned = text.strip()
            if cleaned:
                lines.append(OcrLine(cleaned, float(x), float(y)))
                joined_parts.append(cleaned)
    text_joined = " ".join(joined_parts)
    normalized = normalize_for_match(text_joined)
    keywords = ["ingredienser", "fremgang", "pers", "min", "time", "til servering"]
    keyword_score = sum(normalized.count(word) for word in keywords)
    return OcrImage(
        path=image_path,
        lines=lines,
        text_joined=text_joined,
        keyword_score=keyword_score,
        char_count=len(text_joined),
    )


def choose_recipe_and_meal(images: list[OcrImage]) -> tuple[OcrImage, OcrImage, float]:
    ranked = sorted(images, key=lambda img: (img.keyword_score, img.char_count), reverse=True)
    recipe = ranked[0]
    meal = sorted(images, key=lambda img: (img.keyword_score, img.char_count))[0]
    if meal.path == recipe.path and len(images) > 1:
        meal = ranked[1]
    top = ranked[0]
    second = ranked[1] if len(ranked) > 1 else ranked[0]
    confidence = (top.keyword_score - second.keyword_score) + (top.char_count - second.char_count) / 1000.0
    return recipe, meal, confidence


def parse_int_token(raw: str) -> int | None:
    token = raw.strip()
    if not token:
        return None
    if token.isdigit():
        val = int(token)
        if val > 100 and len(token) == 3 and int(token[1:]) <= 90:
            return int(token[1:])
        return val
    return None


def parse_meta(text: str) -> tuple[int | None, int | None, int | None]:
    normalized = normalize_for_match(text).upper()
    persons = None
    prep = None
    total = None

    m_persons = re.search(r"(\d{1,3})\s*PERS", normalized)
    if m_persons:
        persons = parse_int_token(m_persons.group(1))

    min_matches = re.findall(r"(\d{1,3})\s*MIN", normalized)
    if min_matches:
        candidates = [parse_int_token(tok) for tok in min_matches]
        candidates = [c for c in candidates if c is not None]
        if candidates:
            prep = min(candidates)

    h_match = re.search(r"(\d{1,2})\s*TIME", normalized)
    if h_match:
        hours = parse_int_token(h_match.group(1))
        if hours is not None:
            total = hours * 60

    if total is None and prep is not None:
        total = prep
    return persons, prep, total


def split_columns(lines: list[OcrLine]) -> tuple[list[OcrLine], list[OcrLine], float]:
    if not lines:
        return [], [], 0.0
    x_values = [line.x for line in lines]
    split_x = median(x_values)
    left = [line for line in lines if line.x <= split_x]
    right = [line for line in lines if line.x > split_x]
    left.sort(key=lambda l: (l.y, l.x))
    right.sort(key=lambda l: (l.y, l.x))
    return left, right, split_x


def line_matches(line: str, token: str) -> bool:
    return token in normalize_for_match(line)


def parse_quantity_token(token: str) -> float | None:
    token = token.strip().replace(",", ".")
    fractions = {"1/2": 0.5, "1/4": 0.25, "3/4": 0.75, "½": 0.5, "¼": 0.25, "¾": 0.75}
    if token in fractions:
        return fractions[token]
    try:
        return float(token)
    except ValueError:
        return None


def parse_ingredient_line(raw_line: str) -> dict[str, Any]:
    line = re.sub(r"\s+", " ", raw_line).strip()
    if not line:
        return {"Quantity": None, "Unit": None, "Name": "", "PreparationNote": None}

    m = re.match(r"^([0-9]+(?:[.,][0-9]+)?|[0-9]/[0-9]|½|¼|¾)\s*(.*)$", line)
    if not m:
        return {"Quantity": None, "Unit": None, "Name": line, "PreparationNote": None}

    qty = parse_quantity_token(m.group(1))
    rest = m.group(2).strip()
    if not rest:
        return {"Quantity": qty, "Unit": None, "Name": line, "PreparationNote": None}

    units = [
        "håndfuld",
        "spsk",
        "tsk",
        "skiver",
        "dåse",
        "pose",
        "glas",
        "pakke",
        "kg",
        "g",
        "dl",
        "cl",
        "ml",
        "l",
        "fed",
        "stk",
    ]
    lower = normalize_for_match(rest)
    unit = None
    name_and_note = rest
    for candidate in units:
        if lower.startswith(candidate + " ") or lower == candidate:
            unit = candidate
            name_and_note = rest[len(candidate):].strip()
            break

    if "," in name_and_note:
        name, note = name_and_note.split(",", 1)
        return {
            "Quantity": qty,
            "Unit": unit,
            "Name": name.strip(),
            "PreparationNote": note.strip() or None,
        }

    return {
        "Quantity": qty,
        "Unit": unit,
        "Name": name_and_note.strip(),
        "PreparationNote": None,
    }


def build_markup(lines: list[str]) -> str:
    cleaned = [re.sub(r"\s+", " ", line).strip() for line in lines if line.strip()]
    paragraphs = [f"<p>{line}</p>" for line in cleaned]
    return "<h2>Fremgangsmåde</h2>" + "".join(paragraphs) if paragraphs else "<h2>Fremgangsmåde</h2>"


def extract_recipe_data(ocr: OcrImage, fallback_title: str) -> dict[str, Any]:
    sorted_lines = sorted(ocr.lines, key=lambda l: (l.y, l.x))
    title = ""
    for line in sorted_lines[:8]:
        txt = line.text.strip()
        if len(txt) < 3:
            continue
        if line_matches(txt, "pers") or line_matches(txt, "min") or line_matches(txt, "time"):
            continue
        if line_matches(txt, "ingredienser") or line_matches(txt, "fremgang"):
            continue
        title = txt.title()
        break
    if not title:
        title = fallback_title

    persons, prep_minutes, total_minutes = parse_meta(ocr.text_joined)
    left, right, _ = split_columns(ocr.lines)

    ingred_heading_y = None
    til_servering_y = None
    for line in left:
        normalized = normalize_for_match(line.text)
        if ingred_heading_y is None and "ingredienser" in normalized:
            ingred_heading_y = line.y
        if "til servering" in normalized:
            til_servering_y = line.y

    main_ingredients: list[str] = []
    serving_ingredients: list[str] = []
    for line in left:
        if ingred_heading_y is not None and line.y <= ingred_heading_y:
            continue
        if line_matches(line.text, "ingredienser") or line_matches(line.text, "til servering"):
            continue
        if til_servering_y is not None and line.y > til_servering_y:
            serving_ingredients.append(line.text)
        else:
            main_ingredients.append(line.text)

    fremgang_y = None
    for line in right:
        if "fremgang" in normalize_for_match(line.text):
            fremgang_y = line.y
            break

    instructions_lines: list[str] = []
    note = ""
    for line in right:
        if fremgang_y is not None and line.y <= fremgang_y:
            continue
        txt = line.text.strip()
        if not txt:
            continue
        normalized = normalize_for_match(txt)
        if normalized.startswith("skal der endnu") or txt.startswith("☆"):
            note = txt.replace("☆", "").strip()
            continue
        instructions_lines.append(txt)

    recipe_groups = [
        {
            "Title": "Ingredienser",
            "Ingredients": [parse_ingredient_line(line) for line in main_ingredients if line.strip()],
        }
    ]
    if serving_ingredients:
        recipe_groups.append(
            {
                "Title": "Til servering",
                "Ingredients": [parse_ingredient_line(line) for line in serving_ingredients if line.strip()],
            }
        )

    return {
        "Title": title,
        "SubHeader": None,
        "TotalMinutes": total_minutes,
        "MinutesOfPreparation": prep_minutes,
        "NumberOfPersons": persons,
        "RecipeGroups": recipe_groups,
        "Instructions": build_markup(instructions_lines),
        "Note": note,
        "Keywords": [],
    }


def determine_keywords(title: str) -> list[str]:
    t = normalize_for_match(title)
    keywords: list[str] = []
    if "suppe" in t:
        keywords.append("Suppe")
    if any(word in t for word in ["kylling", "okse", "gullasch", "chorizo", "carne"]):
        if any(word in t for word in ["okse", "gullasch", "carne"]):
            keywords.append("Oksekød")
        if "kylling" in t:
            keywords.append("Kylling")
    else:
        keywords.append("Vegetar")
    return list(dict.fromkeys(keywords))


def main() -> None:
    manifest_lines = read_manifest_lines(MANIFEST)
    grouped: dict[Path, list[Path]] = defaultdict(list)
    for line in manifest_lines:
        image_path = Path(line)
        grouped[image_path.parent].append(image_path)

    index_data = json.loads(INDEX_PATH.read_text(encoding="utf-8"))
    max_existing_id = max((item.get("Id", 0) for item in index_data), default=0)
    next_id = max_existing_id + 1

    engine = RapidOCR()
    review_entries: list[dict[str, Any]] = []
    new_index_entries: list[dict[str, Any]] = []

    for folder in sorted(grouped.keys()):
        images = [path for path in grouped[folder] if path.exists()]
        if not images:
            continue

        ocr_images = [run_ocr(engine, path) for path in images]
        recipe_img, meal_img, confidence = choose_recipe_and_meal(ocr_images)
        fallback_title = sanitize_filename(folder.name) or f"Recipe {next_id}"
        parsed = extract_recipe_data(recipe_img, fallback_title)

        title = sanitize_filename(parsed["Title"]) or fallback_title
        file_base = f"{next_id}. {title}"
        json_path = RECIPES_DIR / f"{file_base}.json"
        meal_ext = meal_img.path.suffix.lower() or ".jpg"
        image_file_name = f"{file_base}{meal_ext}"
        image_out_path = IMAGES_DIR / image_file_name

        recipe_json = {
            "Id": next_id,
            "Title": title,
            "SubHeader": parsed["SubHeader"],
            "TotalMinutes": parsed["TotalMinutes"],
            "MinutesOfPreparation": parsed["MinutesOfPreparation"],
            "NumberOfPersons": parsed["NumberOfPersons"],
            "RecipeGroups": parsed["RecipeGroups"],
            "Instructions": parsed["Instructions"],
            "Note": parsed["Note"],
            "ImageUrl": f"/images/recipe-images/{image_file_name}",
            "Keywords": determine_keywords(title),
        }

        json_path.write_text(json.dumps(recipe_json, ensure_ascii=False, indent=2), encoding="utf-8")
        shutil.copy2(meal_img.path, image_out_path)

        new_index_entries.append(
            {
                "Id": next_id,
                "Title": title,
                "Keywords": recipe_json["Keywords"],
            }
        )

        review_entries.append(
            {
                "id": next_id,
                "folder": str(folder),
                "recipe_image": str(recipe_img.path),
                "meal_image": str(meal_img.path),
                "confidence": round(confidence, 3),
                "title": title,
            }
        )
        next_id += 1

    index_data.extend(new_index_entries)
    INDEX_PATH.write_text(json.dumps(index_data, ensure_ascii=False, indent=2), encoding="utf-8")
    REVIEW_PATH.write_text(json.dumps(review_entries, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Created {len(new_index_entries)} recipes. Review file: {REVIEW_PATH}")


if __name__ == "__main__":
    main()
