Ext.define('WEBAPP.store.BcWhpick', {
    extend: 'Ext.data.Store',
    model: 'WEBAPP.model.BcWhpick',
    pageSize: 200,
    remoteSort: true,
    sorters: [{ property: 'ACT_PICK_USERID', direction: 'DESC' },
        { property: 'STORE_LOC', direction: 'ASC' },
        { property: 'DOCNO', direction: 'ASC' },
        { property: 'SEQ', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/CD0004/All',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }
});