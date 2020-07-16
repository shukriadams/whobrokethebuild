let defaultState = {
    items: []
}

export default function screenshot(state = defaultState, action) {

    switch (action.type) {

        case 'POPULATE_LIST': {
            return Object.assign( { }, state, { items: action.items });
        }

        default:{
            return state;
        }
    }
}