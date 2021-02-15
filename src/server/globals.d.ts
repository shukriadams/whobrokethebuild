interface Window {
    webkitNotifications: any;
}

// declare all global vars used in app so TS-check won't complain about them
declare var __log: {
    info(...args: object)
    debug(...args: object)
    error(...args: object)
    warn(...args: object)
};
declare var _$:string;