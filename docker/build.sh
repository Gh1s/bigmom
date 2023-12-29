#!/bin/bash

while getopts 'spr:' flag;
do
  case "${flag}" in
    s) stable="true"
       echo "Building with stable tag" ;;
    p) push="true"
       echo "Pushing after build";;
    r) IFS="," read -ra additional_registries <<< "${OPTARG}"
       # shellcheck disable=SC2145
       echo "Additionnal registries: ${additional_registries[@]}";;
    *) echo "Unknown parameter passed: $1"; exit 1 ;;
  esac
done

LATEST_TAG=latest
STABLE_TAG=stable

export BUILD_CONTEXT=../
export REGISTRY=repository.csb.nc:5011
export API_REPOSITORY=bigmom/api
export APP_REPOSITORY=bigmom/app
export JOB_INTEGRATION_REPOSITORY=bigmom/jobs/integration
export JOB_EXTRACT_REPOSITORY=bigmom/jobs/extract
export JOB_COLLECTOR_REPOSITORY=bigmom/jobs/collector
export JOB_SPREAD_PWC_REPOSITORY=bigmom/jobs/spread-pwc
export JOB_SPREAD_AMX_REPOSITORY=bigmom/jobs/spread-amx
export JOB_SPREAD_JCB_REPOSITORY=bigmom/jobs/spread-jcb
export API_TAG=$LATEST_TAG
export APP_TAG=$LATEST_TAG
export JOB_INTEGRATION_TAG=$LATEST_TAG
export JOB_EXTRACT_TAG=$LATEST_TAG
export JOB_COLLECTOR_TAG=$LATEST_TAG
export JOB_SPREAD_PWC_TAG=$LATEST_TAG
export JOB_SPREAD_AMX_TAG=$LATEST_TAG
export JOB_SPREAD_JCB_TAG=$LATEST_TAG
echo "Building images with the default '$LATEST_TAG' tag."
docker-compose -f docker-compose-build.yaml build

if [ "$stable" == "true" ]
  then
    echo "Tagging images with the the 'stable' tag."
    docker tag $REGISTRY/$API_REPOSITORY:$LATEST_TAG $REGISTRY/$API_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$APP_REPOSITORY:$LATEST_TAG $REGISTRY/$APP_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_EXTRACT_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$STABLE_TAG
    docker tag $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$STABLE_TAG
fi

dotnet tool restore --tool-manifest ../.config/dotnet-tools.json
export API_VERSION=$(dotnet version -f ../src/Csb.BigMom.Api/Csb.BigMom.Api.csproj | awk 'FNR == 3 {print $1}')
export APP_VERSION=$(dotnet version -f ../src/Csb.BigMom.App/Csb.BigMom.App.csproj | awk 'FNR == 3 {print $1}')
export JOB_INTEGRATION_VERSION=$(dotnet version -f ../src/Csb.BigMom.Job/Csb.BigMom.Job.csproj | awk 'FNR == 3 {print $1}')
export JOB_EXTRACT_VERSION=$(cat ../talend/jobInfo.properties | grep jobVersion | sed 's/jobVersion=//')
export JOB_COLLECTOR_VERSION=$(cat ../scripts/collector/__version__.py | awk '{print $3}' | sed 's/"//g')
export JOB_SPREAD_PWC_VERSION=$(cat ../scripts/spread_pwc/__version__.py | awk '{print $3}' | sed 's/"//g')
export JOB_SPREAD_AMX_VERSION=$(dotnet version -f ../src/Csb.BigMom.Spreading.Amx.Job/Csb.BigMom.Spreading.Amx.Job.csproj | awk 'FNR == 3 {print $1}')
export JOB_SPREAD_JCB_VERSION=$(dotnet version -f ../src/Csb.BigMom.Spreading.Jcb.Job/Csb.BigMom.Spreading.Jcb.Job.csproj | awk 'FNR == 3 {print $1}')
echo "Tagging images with their version tag."
docker tag $REGISTRY/$API_REPOSITORY:$LATEST_TAG $REGISTRY/$API_REPOSITORY:$API_VERSION
docker tag $REGISTRY/$APP_REPOSITORY:$LATEST_TAG $REGISTRY/$APP_REPOSITORY:$APP_VERSION
docker tag $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$JOB_INTEGRATION_VERSION
docker tag $REGISTRY/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_EXTRACT_REPOSITORY:$JOB_EXTRACT_VERSION
docker tag $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$JOB_COLLECTOR_VERSION
docker tag $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$JOB_SPREAD_PWC_VERSION
docker tag $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$JOB_SPREAD_AMX_VERSION
docker tag $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$JOB_SPREAD_JCB_VERSION

if [ "${#additional_registries[@]}" -gt "0" ]
  echo "Tagging images with the additional registries."
  then
    for additional_registry in "${additional_registries[@]}"; do
      docker tag $REGISTRY/$API_REPOSITORY:$LATEST_TAG $additional_registry/$API_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$APP_REPOSITORY:$LATEST_TAG $additional_registry/$APP_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG
      docker tag $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG
      if [ "$stable" == "true" ]
        then
          docker tag $REGISTRY/$API_REPOSITORY:$LATEST_TAG $additional_registry/$API_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$APP_REPOSITORY:$LATEST_TAG $additional_registry/$APP_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_INTEGRATION_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_EXTRACT_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_COLLECTOR_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_PWC_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_AMX_REPOSITORY:$STABLE_TAG
          docker tag $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_JCB_REPOSITORY:$STABLE_TAG
      fi
      docker tag $REGISTRY/$API_REPOSITORY:$LATEST_TAG $additional_registry/$API_REPOSITORY:$API_VERSION
      docker tag $REGISTRY/$APP_REPOSITORY:$LATEST_TAG $additional_registry/$APP_REPOSITORY:$APP_VERSION
      docker tag $REGISTRY/$JOB_INTEGRATION_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_INTEGRATION_REPOSITORY:$JOB_INTEGRATION_VERSION
      docker tag $REGISTRY/$JOB_EXTRACT_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_EXTRACT_REPOSITORY:$JOB_EXTRACT_VERSION
      docker tag $REGISTRY/$JOB_COLLECTOR_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_COLLECTOR_REPOSITORY:$JOB_COLLECTOR_VERSION
      docker tag $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_PWC_REPOSITORY:$JOB_SPREAD_PWC_VERSION
      docker tag $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_AMX_REPOSITORY:$JOB_SPREAD_AMX_VERSION
      docker tag $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY:$LATEST_TAG $additional_registry/$JOB_SPREAD_JCB_REPOSITORY:$JOB_SPREAD_JCB_VERSION
    done
fi

if [ "$push" == "true" ]
then
  echo "Pushing images."
  docker push $REGISTRY/$API_REPOSITORY --all-tags
  docker push $REGISTRY/$APP_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_INTEGRATION_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_EXTRACT_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_COLLECTOR_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_SPREAD_PWC_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_SPREAD_AMX_REPOSITORY --all-tags
  docker push $REGISTRY/$JOB_SPREAD_JCB_REPOSITORY --all-tags
  if [ "${#additional_registries[@]}" -gt "0" ]
    echo "Pushing images of the additional registries."
    then
      for additional_registry in "${additional_registries[@]}"; do
        docker push $additional_registry/$API_REPOSITORY --all-tags
        docker push $additional_registry/$APP_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_INTEGRATION_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_EXTRACT_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_COLLECTOR_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_SPREAD_PWC_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_SPREAD_AMX_REPOSITORY --all-tags
        docker push $additional_registry/$JOB_SPREAD_JCB_REPOSITORY --all-tags
      done
  fi
fi