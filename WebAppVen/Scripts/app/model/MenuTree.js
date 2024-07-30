Ext.define('WEBAPP.model.MenuTree', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'id', type: 'string' },
        { name: 'FG', type: 'string' },
        { name: 'PG', type: 'string' },
        { name: 'FS', type: 'string' },
        { name: 'text', type: 'string' },
        { name: 'url', type: 'string' }
    ]
});