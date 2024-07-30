Ext.define('T1Model', {
    extend: 'Ext.data.Model',
    fields: ['VID', 'TMPKEY', 'KONZS', 'LIFNR', 'NAME1', 'NAME2', 'POST_CODE1', 'ADDR', 'COUNTRY', 'TEL_NUMBER', 'TEL_EXTENS', 'FAX_NUMBER', 'FAX_EXTENS', 'SMTP_ADDR', 'VBUND', 'STCEG', 'RMARK', 'PMARK', 'GMARK', 'STATUS', 'CHG_DATE']
});

var T1Store = Ext.create('Ext.data.Store', {
    model: 'T1Model',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'VID', direction: 'ASC' }],

    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: Ext.getCmp('konzs').getValue(),
                p2: Ext.getCmp('stceg').getValue(),
                p3: Ext.getCmp('name1').getValue(),
                p4: Ext.getCmp('name2').getValue(),
                p5: Ext.getCmp('lifnr').getValue()

            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/Query',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var ExcelStore = Ext.create('Ext.data.Store', {
    model: 'T1Model',
    //pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'VID', direction: 'ASC' }],

    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: Ext.getCmp('konzs').getValue(),
                p2: Ext.getCmp('stceg').getValue(),
                p3: Ext.getCmp('name1').getValue(),
                p4: Ext.getCmp('name2').getValue()

            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/QueryExcel',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});