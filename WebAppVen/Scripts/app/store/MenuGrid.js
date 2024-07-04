Ext.define('WEBAPP.store.MenuGrid', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.MenuGrid',
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/Menu/Query',
        reader: {
            type: 'json',
            rootProperty: 'etts'
        }
    }
});