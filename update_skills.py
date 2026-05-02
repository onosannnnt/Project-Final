import os
import re

guid = "8d69cc920a01e47d5b53c9c1598e73c3"
root_dirs = {
    "Assets/_Scripts/Skills/RV": 1,
    "Assets/_Scripts/Skills/EL": 2,
    "Assets/_Scripts/Skills/CE": 3,
    "Assets/_Scripts/Skills/EnemySkill": 0
}

updated_files = []

for root_dir, style_value in root_dirs.items():
    if not os.path.exists(root_dir):
        print(f"Directory not found: {root_dir}")
        continue
    
    for root, dirs, files in os.walk(root_dir):
        for file in files:
            if file.endswith(".asset"):
                file_path = os.path.join(root, file)
                try:
                    with open(file_path, "r", encoding="utf-8") as f:
                        content = f.read()
                    
                    if guid in content:
                        # Check if skillStyle already exists
                        if "skillStyle:" in content:
                            new_content = re.sub(r"(skillStyle:\s*)\d+", rf"\g<1>{style_value}", content)
                        else:
                            # Add after skillType: \d+
                            new_content = re.sub(r"(skillType:\s*\d+)", rf"\1\n  skillStyle: {style_value}", content)
                        
                        if new_content != content:
                            with open(file_path, "w", encoding="utf-8", newline="\n") as f:
                                f.write(new_content)
                            updated_files.append(file_path)
                except Exception as e:
                    print(f"Error processing {file_path}: {e}")

print(f"Total files updated: {len(updated_files)}")
for f in updated_files:
    print(f)
