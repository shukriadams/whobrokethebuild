- need daemon that autocleans orphans
- rename "hasUI" to "hadAdminUI" vs "hasUserUI"
- need a system for wiping and rebuilding delta on a build after a given build if that build was reset
- need a system for wiping and reprocessing logs, and then fault calculation from logs
- time for log parse needs to be more use-friendly
- need "watcher" - alert on all build status changes, regardless of who was responsible for them
- need function to assign a failing build to one or more people
- ensure that build error change works, and that this is adequetyly warned for
- unreal log parser needs to be able to catch "system" errors, build machine state errors, CIserver internal errors
- add non-unique lookup indexes to cover all query fields in mongo
- need function to wipe out all builds after a given build
- port to typescipt, current complexity is unworking
- change updates so we send only explicitly set properties back to db (instead of entire object)
- routes should use logic layer instead of containing logic themselves
- harden plugin system - define required-by-category, single-by-category
- add next/previous links to build page
- add plugin flush function to force plugin reinit
- warn if a more than one user have identical VCServer mappings, but allow
- add option to wipe and reset user mapping for a given build, this is needed when a user mapping is changed and data must be reboun / reprocessed for that build
- recent builds order must be reversed to latest first
- user recenty builds shoudld state that no builds can be shown if no mapping is available
- do not parse error logs for builds that are not failing
- change sort order of builds without delta to oldest first, this should speed up processing of concurrent builds
- need several hundreds of records per build, across multiple builds, to test performance
- need page for build agents showing history on that machine
- need a way to tag changes or build errors based on the kind of files were changed in a build, or the kind of files which appeared in the error log. eg : cooking errors, blueprint errors, missing files.
- need build stats : fails vs working %, average time a build stays broken for, nr of builds involving a tag / error type, etc
- need even simpler job page that shows only delta - build working vs failing, and buils that changed delta.
- need to mark builds for which specific revisions cannot be retrieved. We need to "soft" link these to a revision range. In the case of jenkins, there is no "changeSet" in the object root
- prevents multiple users from binding to the same vcs source name 

LATER
- add caching
- write postgres layer with https://sequelize.org/

DONE
- add link back to original CISysem to build pages
- all API and log calls from CI should be cached locally
- improved paging : break pages into groups, show active page