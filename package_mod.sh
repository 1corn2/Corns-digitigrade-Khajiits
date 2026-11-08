#!/bin/sh

OS_EXPORT_DIR="./OS-export"

if [ ! -d "${OS_EXPORT_DIR}" ]; then
	echo "Please export the project to ${OS_EXPORT_DIR}"
	exit 1
fi

mkdir output
TMPDIR=$(mktemp -d)
PROJECTDIR="${PWD}"

cd "${TMPDIR}" || exit 2
cp -r "${PROJECTDIR}/${OS_EXPORT_DIR}"/* .
cp -r "${PROJECTDIR}/SNIFF-fixes/"* "./CalienteTools/BodySlide/ShapeData/"
cp "${PROJECTDIR}/Corns Khajiits/corns_khajiits.esp" .
zip -9 -r "${PROJECTDIR}/output/CornsKhajiits.zip" .
7z a -mx9 "${PROJECTDIR}/output/CornsKhajiits.7z" .
