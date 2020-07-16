import React, { Fragment } from 'react';
import { Link } from 'react-router-dom';
import { connect } from 'react-redux';
import { populateList } from './../actions/list';


class View extends React.Component {

    async loadList(){
        populateList();
    }

    render(){
        return(
            <Fragment>
                <button onClick={this.loadList.bind(this)}>Load list</button>

                <ul>
                    {
                        this.props.items.map((item)=>{
                            return(                            
                                <li key={item}>
                                    <Link to={`/item/${item}`}>{item}</Link>
                                </li>
                            )
                        })
                    }
                </ul>
                
            </Fragment>
        );
    }
}

View.defaultProps = {

};

View = connect(
    function (state, ownProps) {
        return {
            items : state.list.items
        }
    }
)(View);

export { View };
