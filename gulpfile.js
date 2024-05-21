var gulp = require("gulp");
var msbuild = require("gulp-msbuild");
var path = require("path");

gulp.task("build", () => {
    return gulp.src(path.join(__dirname, "WinFormsApp1", "WinFormsApp1.sln"))
            .pipe(msbuild());
});

gulp.task("default", gulp.series("build"));
