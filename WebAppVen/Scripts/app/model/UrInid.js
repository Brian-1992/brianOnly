Ext.define('WEBAPP.model.UrInid', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'INID', type: 'string' },
        { name: 'INID_NAME', type: 'string' },
        { name: 'INID_OLD', type: 'string' },
        { name: 'INID_FLAG', type: 'string' },
        { name: 'INID_FLAG_NAME', type: 'string' },
        { name: 'CREATE_TIME', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});