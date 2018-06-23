SELECT Translation_hint.Chinese, Props_total_table.* FROM "Props_total_table" JOIN "Translation_hint" ON Props_total_table.Props_Name = Translation_hint.ID
WHERE Props_total_table.Item_Type IN ('Material')
AND Translation_hint.Chinese LIKE "飞机%"

-- SELECT Translation_hint.Chinese,ExhibitData.*, Props_total_table.* FROM "Props_total_table" JOIN "Translation_hint", ExhibitData ON Props_total_table.Props_Name = Translation_hint.ID and ExhibitData.itemId = Props_total_table.Props_Id
-- -- WHERE ExhibitData.type = '6' ORDER BY ExhibitData.size
-- WHERE Translation_hint.Chinese LIKE "旧水%"