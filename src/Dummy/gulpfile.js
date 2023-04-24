//required gulp modules
var gulp = require('gulp');
var sass = require('gulp-sass')(require('node-sass'));
var sourcemaps = require('gulp-sourcemaps');
var autoprefixer = require('gulp-autoprefixer');
var concat = require('gulp-concat');
var rename = require('gulp-rename');
var uglify = require('gulp-uglify-es').default;
var copy = require('gulp-copy');

//variables
var scssAssets = 'assets/styles/src/**/*.scss';
var cssOutPath = 'assets/styles/';
var jsAssets = 'assets/scripts/src/**/*.js';
var jsOutPath = 'assets/scripts/';

//sub-tasks
gulp.task('minified-styles', gulp.series(function runScssCompilerMin(cb) {
    gulp.src([scssAssets])
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(autoprefixer())
        .pipe(rename({ extname: '.min.css' }))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(cssOutPath));
    cb();
}));
gulp.task('non-minified-styles', gulp.series(function runScssCompiler(cb) {
    gulp.src([scssAssets])
        .pipe(sourcemaps.init())
        .pipe(sass({ outputStyle: 'expanded' }).on('error', sass.logError))
        .pipe(autoprefixer())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(cssOutPath));
    cb();
}));
gulp.task('minified-scripts', gulp.series(function runJsConcatenateMin(cb) {
    gulp.src([jsAssets])
        .pipe(sourcemaps.init())        
        .pipe(uglify({ compress: { hoist_funs: false } }))
        .pipe(concat('main.min.js'))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(jsOutPath));
    cb();
}));
gulp.task('non-minified-scripts', gulp.series(function runJsConcatenate(cb) {
    gulp.src([jsAssets])
        .pipe(sourcemaps.init())
        .pipe(concat('main.js'))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(jsOutPath));
    cb();
}));
gulp.task('copy-scripts', gulp.series(function copyFiles(cb){
    gulp.src(['node_modules/perfect-scrollbar/dist/*.js'])
        .pipe(copy(jsOutPath, { prefix: 3 }));
    cb();
}));

//main-tasks
gulp.task('styles', gulp.series('non-minified-styles', 'minified-styles'));
gulp.task('scripts', gulp.series('non-minified-scripts', 'minified-scripts', 'copy-scripts'));
gulp.task('all', gulp.series('styles', 'scripts'));

//default-task
gulp.task('default', gulp.series('all', function startWatcher() {
    gulp.watch(scssAssets, gulp.series('styles'));
    gulp.watch(jsAssets, gulp.series('scripts'));
}));
