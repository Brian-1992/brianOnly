Ext.define('WEBAPP.model.UR_DOC', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'DK', type: 'string' },
        { name: 'DN', type: 'string' },
        { name: 'DD', type: 'string' },
        { name: 'UK', type: 'string' },
        { name: 'CREATE_TIME', type: 'date' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'date' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});