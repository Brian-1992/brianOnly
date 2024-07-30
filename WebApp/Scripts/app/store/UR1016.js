Ext.define('WEBAPP.store.UR1016', {
    extend: 'WEBAPP.data.RemoteStore',
    model: 'WEBAPP.model.UR_DOC',
    pageSize: 20,
    autoLoad: false,
    remoteSort: true,
    sorters: [{ property: 'DK', direction: 'ASC' }],
    proxy: {
        url: '/api/UR1016/All'
    }
});