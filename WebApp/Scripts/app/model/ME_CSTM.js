Ext.define('WEBAPP.model.ME_CSTM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'CSTM', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_ID', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_ID', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});