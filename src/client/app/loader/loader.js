import React, { Fragment } from 'react';

class View extends React.Component {

    constructor(props){
        super(props);
        this.state = {
            data : null
        }
    }

    async getData(props){
        let data = await props.action();
        let newState = {};
        newState[props.dataAttribute] = data;
        this.setState(newState);
    }

    async componentWillReceiveProps(props){
        await this.getData(props);
    }

    async componentDidMount(){
        await this.getData(this.props);
    }

    render(){
        return(
            <Fragment>
                {
                    !this.state[this.props.dataAttribute]   && 
                        <div>
                            Wait for data ...
                        </div>
                }

                {
                    this.state[this.props.dataAttribute]   && 
                        <Fragment>
                             {React.cloneElement(this.props.children, this.state)}
                        </Fragment>
                }
            </Fragment>
        );
    }
}

View.defaultProps = {
    dataAttribute : 'myData' // set this to the name of the data data property you want in the child view
};

export { View };
