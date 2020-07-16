import store from './../store';
import watch from 'redux-watch';

(function(){
    store.subscribe(watch(store.getState, 'list')(function(items) {
        console.log('store.list is ' + items);
    }));
})()

