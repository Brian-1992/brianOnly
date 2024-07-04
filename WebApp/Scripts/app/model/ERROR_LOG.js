
Ext.define('WEBAPP.model.ERROR_LOG', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'LOGTIME', type: 'string' },
        { name: 'PG', type: 'string' },
        { name: 'MSG', type: 'string' },
        { name: 'USERID', type: 'string' }
    ]
});