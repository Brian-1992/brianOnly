Ext.define('WEBAPP.store.MenuTreeMobile', {
    extend: 'Ext.data.TreeStore',
    model: 'WEBAPP.model.MenuTree',
    defaultRootProperty: 'etts',
    nodeParam: 'PG',
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/Menu/Mobile'
    }
});