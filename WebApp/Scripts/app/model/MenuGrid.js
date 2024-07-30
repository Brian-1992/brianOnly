Ext.define('WEBAPP.model.MenuGrid', {
    extend: 'Ext.data.Model',
    fields: [
        { name: 'id', type: 'string' },
        { name: 'text', type: 'string' },
        { name: 'url', type: 'string' }
    ]
});