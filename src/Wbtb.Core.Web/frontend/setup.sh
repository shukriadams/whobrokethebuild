set -e



mkdir -p  ./components/bootstrip

VERSION="0.0.19"

curl -L https://github.com/shukriadams/bootstrip/releases/download/$VERSION/bootstrip.js --output ./components/bootstrip/bootstrip.js
curl -L https://github.com/shukriadams/bootstrip/releases/download/$VERSION/bootstrip.css --output ./components/bootstrip/bootstrip.scss
curl -L https://github.com/shukriadams/bootstrip/releases/download/$VERSION/bootstrip-theme-default.css --output ./components/bootstrip/bootstrip-theme-default.scss
curl -L https://github.com/shukriadams/bootstrip/releases/download/$VERSION/bootstrip-theme-darkmoon.css --output ./components/bootstrip/bootstrip-theme-darkmoon.scss 