/**
 * Single wrapper for history object
 * Use:
 *
 * import {history} from './history';
 * history.push('myurl/myParamenters')
 * 
 * use changes to track if history back is possible
 */
 
import createBrowserHistory from 'history/createBrowserHistory'
let changes = 0;
let history = createBrowserHistory();

history.listen(()=>{
    changes ++;
});

export { history, changes };

