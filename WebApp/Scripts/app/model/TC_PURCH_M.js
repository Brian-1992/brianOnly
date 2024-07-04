Ext.define('WEBAPP.model.TC_PURCH_M', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'PUR_NO', type: 'string' },
        { name: 'PUR_DATE', type: 'string' },
        { name: 'TC_TYPE', type: 'string' },
        { name: 'PUR_UNM', type: 'string' },
        { name: 'PURCH_ST', type: 'string' },
        { name: 'PUR_NOTE', type: 'string' },


        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});