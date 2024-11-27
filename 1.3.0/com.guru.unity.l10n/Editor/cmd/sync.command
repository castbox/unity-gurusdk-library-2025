#!/bin/bash
# switch into workdir
DIR=$(cd $(dirname $0); pwd)
cd ${DIR}
echo [DIR] ${DIR}
# import params
source params
# print params
echo [1] - FILE:  ${FILE}
echo [2] - SHEET_ID:  ${SHEET_ID}
echo [3] - TABLE_NAME:  ${TABLE_NAME}
echo [4] - ACTION:  ${ACTION}
echo [5] - LOG: ${LOG}
# sync and sync_translate action
if [ "${ACTION}" = "sync" ] || [ "${ACTION}" = "sync_translate" ] ; then
    echo "**** l10n ${ACTION} start ****"
    ./l10n ${ACTION} --spreadsheet_id ${SHEET_ID} --table ${TABLE_NAME} --platform unity --path ./${FILE} > ${LOG}
    echo "**** l10n ${ACTION} over ****"
# translate action
elif [ "${ACTION}" = "translate" ]; then
    echo "**** l10n ${ACTION} start ****"
    ./l10n ${ACTION} --spreadsheet_id ${SHEET_ID} --table ${TABLE_NAME} > ${LOG}
    echo "**** l10n ${ACTION} over ****"
else
    echo "**** Unknown Action: ${ACTION}. Exit ****"
    exit -1
fi