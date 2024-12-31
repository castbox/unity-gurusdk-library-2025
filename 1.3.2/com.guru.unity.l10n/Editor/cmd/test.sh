#!/bin/bash

SHEET_ID=1A3Swcu1Y4Bm58OBNIuP1i2l1V08TFMt0GXExEzrLe_g
TABLE_NAME=Main

echo "----- Start Translation -----"

#curl --location "http://google-sheet.translator.saas.castbox.fm/api.v1/spreadsheet/uniform?id=${SHEET_ID}&name=${TABLE_NAME}"

./l10n translate --spreadsheet_id ${SHEET_ID} --table ${TABLE_NAME}

echo "----- Translation Over -----"
