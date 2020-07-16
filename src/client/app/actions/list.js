import store from './../store/store';
import { getJsonAsync } from './../helpers/ajax';

let populateList = async function(){ 
    let list = await getJsonAsync('/list');
    return store.dispatch({ type: 'POPULATE_LIST', items : list }); 
} 

export {
    populateList
}