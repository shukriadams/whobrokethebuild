import React, { Fragment } from 'react';
import { connect } from 'react-redux';

class View extends React.Component {
    render(){
        return(
            <Fragment>
                <h2>Item</h2>
                <div>
                    Data :{this.props.data}
                </div>
                <div>
                    ItemId : {this.props.itemId}
                </div>
            </Fragment>
        );
    }
}

View.defaultProps = {

};

View = connect(
    function (state, ownProps) {
        return {
            
        }
    }
)(View);

export { View };
