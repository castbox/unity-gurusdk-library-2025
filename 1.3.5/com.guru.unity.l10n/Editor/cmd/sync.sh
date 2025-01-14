#!/usr/bin/sh

FILE=$1
SPREAD_SHEET=$2
TABLE=$3
ACTION=$4

#./l10n sync --platform unity --path ./${FILE} --spreadsheet_id ${SPREAD_SHEET} --table ${TABLE}
./l10n ${ACTION} --platform unity --path ./${FILE} --spreadsheet_id ${SPREAD_SHEET} --table ${TABLE}
echo "**** l10n sync over ****"