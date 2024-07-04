Ext.define('WEBAPP.model.CHK_WHID', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'WH_NO', type: 'string' },
        { name: 'WH_USERID', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
    ]
});