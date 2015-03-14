/*global module */
module.exports = function (grunt) {
    'use strict';
    grunt.initConfig({
        bower: {
            install: {
                options: {
                    targetDir: "app/lib",
                    layout: "byComponent",
                    cleanTargetDir: false
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-bower-task');
};