import { View as Default } from './../default/default';
import { View as Item } from './../item/item';
import React from 'react';
import ReactDOM from 'react-dom';
import { Router, Switch, Route, Link } from 'react-router-dom'
import { history } from './../helpers/history';
import Store from './../store/store'
import { Provider } from 'react-redux'
import { populateList } from './../actions/list';
import socketInitialize from './../helpers/socket';
import { Layout } from './../layout/layout';
import {View as Loader } from './../loader/loader';
import listWatcher from './../store/watchers/list'; // importing watcher will start it
import pubsub from './../helpers/pubsub';

function doLongThing(itemId){
    return new Promise(function(resolve,){
        window.setTimeout(()=>{
            resolve({
                data : 'I work with ' + itemId
            });
        }, 2000)

    })
}

async function dataLoader(itemId){
    let data = await doLongThing(itemId);
    return data;
}



(async function(){
    pubsub.once('main', 'onRehydrated', async ()=>{
        ReactDOM.render(
            <Router history={history}>
                <Provider store={Store}>
                    <Switch>
                        <Layout>
                            <Route exact path="/" component={Default} />
                            <Route exact path="/item/:itemId" render={props => 
                                <Loader dataAttribute="build" action={()=>{ return dataLoader(props.match.params.itemId) }}>
                                    <Item {...props.match.params} /> 
                                </Loader>
                            } />
                        </Layout>
                    </Switch>
                </Provider>
            </Router>,
            document.getElementById('reactHook')
        );
    
        socketInitialize();

        // do starting data fetches here
        await populateList()        
    });
}())
