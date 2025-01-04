#!/bin/bash

DIR=$(cd $(dirname $0); pwd)
cd ${DIR}

echo "--- CMD Trans at ${DIR}"
# 导入参数
source params

echo "-[1]- SHEET_ID ${SHEET_ID}"
echo "-[2]- TABLE_NAME ${TABLE_NAME}"
echo "-[3]- ACTION ${ACTION}"
echo "----- LOG ${LOG}"

./l10n ${ACTION} --spreadsheet_id ${SHEET_ID} --table ${TABLE_NAME} > ${LOG}

echo "**** l10n translate over ****"