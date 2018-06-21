SELECT 
(SELECT Chinese FROM Translation_hint WHERE Translation_hint.ID=name)as nameCN,
(SELECT Chinese FROM Translation_hint WHERE Translation_hint.ID=description)as descriptionCN,
* FROM "Achievement" 