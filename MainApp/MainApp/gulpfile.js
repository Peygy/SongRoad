const { src, dest, watch, lastRun, series } = require("gulp");

var sass = require('gulp-sass')(require('sass')),
    group_media = require("gulp-group-css-media-queries"),
    clean_css = require("gulp-clean-css"),
    rename = require("gulp-rename"),
    browserSync = require("browser-sync").create(),
    uglify = require('gulp-uglify-es').default;

var paths = {
    sass: "./wwwroot/sass/**/*.scss",
    css: "./wwwroot/css",

    js: "./wwwroot/js/*.js",
    jsmin: "./wwwroot/js/min",

    cshtml: "./Views/**/*cshtml"
}


function CsHtml() {
    return src(paths.cshtml, {since: lastRun(CsHtml)})
        .pipe(browserSync.stream())
}
exports.CsHtml = CsHtml;

function Sass() {
    return src(paths.sass)
        .pipe(sass())
        .pipe(group_media())
        .pipe(clean_css())
        .pipe(
            rename({
                extname: ".min.css"
            })
        )
        .pipe(dest(paths.css))
        .pipe(browserSync.stream())
}
exports.Sass = Sass;

function JS() {
    return src(paths.js)
    .pipe(uglify())
    .pipe(
        rename({
            extname: ".min.js"
        })
    )
    .pipe(dest(paths.jsmin))
    .pipe(browserSync.stream())
}
exports.JS = JS;

function myServer() {
    var files = [
        paths.cshtml,
        paths.sass,
        paths.css
    ];

    browserSync.init(files, {
        proxy: 'localhost:5148',
        online: true,
        notify: false
    });

    watch(paths.sass, {usePolling: true}, Sass).on('change', browserSync.reload);
    watch(paths.js, {usePolling: true}, JS).on('change', browserSync.reload);
    watch(paths.cshtml, {usePolling: true}, CsHtml).on('change', browserSync.reload);
}
exports.myServer = myServer;

exports.default = series(Sass, JS, CsHtml, myServer);