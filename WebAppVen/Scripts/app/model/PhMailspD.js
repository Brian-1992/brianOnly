Ext.define('WEBAPP.model.PhMailspD', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'MGROUP', type: 'string' },
        { name: 'MSGNO', type: 'string' },
        { name: 'AGEN_NO', type: 'string' },
        { name: 'M_CONTID', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' }
    ]
});