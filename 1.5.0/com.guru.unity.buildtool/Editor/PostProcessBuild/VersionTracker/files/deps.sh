#!/bin/bash
# version 1.2.0 - by HuYufei
# import env variables
source .deps_env

if [ "${PLATFORM}" = "android" ]; then
    gradle w
fi

echo "----- collect deps start-----"

depaudit --platform "${PLATFORM}" --dir "${DIR}" --build_version "${BUILD_NAME}" --app_name "${APP_NAME}" --app_id "${APP_ID}" --engine unity

echo "----- collect deps over -----"

if [ "${PLATFORM}" = "android" ]; then
  
  if [ -f ${DIR}/gradlew ]; then
      ${DIR}/gradlew --stop
      echo "***************** deps collect success! *****************"
  else
      echo "***************** gradlew not found, deps collect failed! *****************"
  fi
  
else
  echo "***************** deps collect success! *****************"
fi