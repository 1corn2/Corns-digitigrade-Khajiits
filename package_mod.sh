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
cp -r "${PROJECTDIR}/SNIFF-fixes/"* "./CalienteTools/BodySlide/ShapeData/" || exit 3
cp "${PROJECTDIR}/Corns Khajiits/corns_khajiits.esp" .
# Do not grab the addon group file for first export
echo "Packaging base mod..."
find .  -type f \( ! -name 'HIMBO*' \) -o -name 'HIMBOVanilla*' | zip -9  "${PROJECTDIR}/output/CornsKhajiits.zip" -@
echo "Packaging HIMBO+SOS addon..."
find .  -type f -name 'HIMBOSOS*' -o -name 'HIMBO*xml' | zip -9  "${PROJECTDIR}/output/CornsKhajiits_HimboAddon.zip" -@
