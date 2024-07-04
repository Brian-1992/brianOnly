Ext.define('T1Model', {
    extend: 'Ext.data.Model',
    fields: ['VID', 'TMPKEY', 'KONZS', 'NAME1', 'NAME2', 'POST_CODE1', 'ADDR', 'COUNTRY', 'TEL_NUMBER', 'TEL_EXTENS', 'FAX_NUMBER', 'FAX_EXTENS', 'SMTP_ADDR', 'VBUND', 'STCEG', 'RMARK', 'PMARK', 'GMARK', 'STATUS', 'CHG_DATE']
});

Ext.define('T2Model', {
    extend: 'Ext.data.Model',
    fields: ['TUSER', 'RLNO']
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
                p4: Ext.getCmp('name2').getValue()

            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/QueryVerify',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var T2Store = Ext.create('Ext.data.Store', {
    model: 'T2Model',
    //pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'TUSER', direction: 'ASC' }],

    listeners: {
        load: function (store, records, successful, eOpts) {
            Ext.getCmp('RLNO').setValue(records[0].data['RLNO']);
            T1Store.load();

        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetRLNO',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});