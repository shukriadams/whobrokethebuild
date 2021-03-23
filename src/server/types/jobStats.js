/**
 * @typedef {Object} JobStats
 * @property {number} totalBreaks Total number of times this job has broken since it was first run
 * @property {number} totalRuns Total number of times this job has run
 * @property {number} daysActive Number of days this job has existed
 * @property {number} runsPerDay Average number of times this job runs per day
 * @property {number} breaksPerDay Average number of times this job breaks per day
 * @property {number} breakRatio raw runs-to-fail ratio 
 * @property {number} daysSinceLastBreak 
 * @property {number} breaksThisWeek 
 * @property {number} breaksThisMonth 
 */
module.exports = class JobStats {

    constructor(){
        this.totalBreaks = 0
        this.totalRuns = 0
        this.daysActive = 0
        this.runsPerDay = 0
        this.breaksPerDay = 0
        this.daysSinceLastBreak = 0
        this.breakRatio = 0 
        this.breaksThisWeek = 0
        this.breaksThisMonth = 0
    }
    
}