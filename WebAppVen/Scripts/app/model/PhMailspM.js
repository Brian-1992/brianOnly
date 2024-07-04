Ext.define('WEBAPP.model.PhMailspM', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'INID', type: 'string' }, 
        { name: 'MGROUP', type: 'string' }, 
        { name: 'MSGNO', type: 'string' },
        { name: 'MSGTEXT', type: 'string' },
        { name: 'CREATE_TIME', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_TIME', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string'   }
    ]
});