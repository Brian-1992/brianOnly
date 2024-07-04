Ext.define('WEBAPP.store.MenuTree', {
    extend: 'Ext.data.TreeStore',
    model: 'WEBAPP.model.MenuTree',
    defaultRootProperty: 'etts',
    nodeParam: 'PG',
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/Menu/Main'
    }
});