Ext.define('fieldModel', {
    extend: 'Ext.data.Model',
    fields: ['VID', 'KONZS', 'NAME1', 'TMP_NAME1', 'NAME2', 'TMP_NAME2', 'POST_CODE1', 'ADDR', 'TMP_ADDR', 'TEL_NUMBER', 'FAX_NUMBER', 'SMTP_ADDR', 'URL', 'STCEG', 'COUNTRY', 'RMARK', 'PMARK', 'GMARK', 'STATUS',
        'CHG_DATE', 'J_1KFREPRE', 'ADDRC', 'ADDRS', 'CORPORATION']
});

// 聯絡人清單
Ext.define('T1Model', {
    extend: 'Ext.data.Model',
    fields: ['VID5', 'KONZS', 'PAVIP', 'ABTNR', 'PAFKT', 'NAME_LAST', 'NAME_FIRST', 'FULLNAME', 'PARAU', 'TEL_NUMBER', 'TEL_EXTENS', 'TEL_MEMO',
        'MOB_NUMBER', 'MOB_MEMO', 'TEL_NUMBER2', 'TEL_EXTENS2', 'TEL_MEMO2', 'MOB_NUMBER2', 'MOB_MEMO2', 'FAX_NUMBER', 'FAX_EXTENS', 'FAX_MEMO', 'FAX_NUMBER2',
        'FAX_EXTENS2', 'FAX_MEMO2', 'SMTP_ADDR', 'SMTP_ADDR2', 'OPEN1', 'RMARK', 'PMARK', 'GMARK', 'FMARK', 'USERID', 'ADMIN1', 'CHG_DATE']
});

// 可報價物料清單
Ext.define('T2Model', {
    extend: 'Ext.data.Model',
    fields: ['MATNR', 'LIFNR', 'LTSNR', 'QA_LEVEL', 'PUR_LEVEL', 'CHG_DATE', 'MAKTX', 'NAME1', 'NAME3', 'MFRNC', 'MFRNE', 'OEMNC', 'OEMNE']
});

// 供應商代碼 123類
//Ext.define('T3Model', {
//    extend: 'Ext.data.Model',
//    fields: ['VID1', 'KONZS', 'LIFNR', 'BUKRS', 'AKONT', 'FDGRV', 'ZTERM', 'REPRF', 'ZWELS', 'EKORG', 'WAERS', 'STATUS', 'CHG_DATE']
//});
// 採購經辦需填寫的欄位 科目群組 1,2,3
Ext.define('T3Model', {
    extend: 'Ext.data.Model',
    fields: ['VID1', 'KONZS', 'LIFNR', 'KTOKK', 'ZTERM', 'ZWELS', 'WAERS', 'INCO1', 'INCO2', 'CHG_DATE', 'ZTERM_TEXT', 'ZWELS_TEXT', 'INCO1_TEXT', 'TMP_ZTERM', 'TMP_ZTAGG', 'TMP_ZWELS']
});

// 國別
Ext.define('T4Model', {
    extend: 'Ext.data.Model',
    fields: ['LAND1', 'LANDX', 'NATIO', 'LANDX50', 'NATIO50', 'PRQ_SPREGT']
});

Ext.define('T5Model', {
    extend: 'Ext.data.Model',
    fields: ['ZLSCH', 'TEXT1']
});

// 國貿條件
Ext.define('T6Model', {
    extend: 'Ext.data.Model',
    fields: ['INCO1', 'TEXT1']
});

// 付款條件
Ext.define('T7Model', {
    extend: 'Ext.data.Model',
    fields: ['ZTERM', 'ZTAGG', 'TEXT1']
});

// 簽核歷程
Ext.define('T8Model', {
    extend: 'Ext.data.Model',
    fields: ['VID', 'KONZS', 'SENDER', 'RECEIVER', 'ACTION', 'COMMENT', 'DATE']
});

// 部門
Ext.define('T9Model', {
    extend: 'Ext.data.Model',
    fields: ['ABTNR', 'TEXT1']
});

// 功能 職稱
Ext.define('T10Model', {
    extend: 'Ext.data.Model',
    fields: ['PAFKT', 'TEXT1']
});

Ext.define('T11Model', {
    extend: 'Ext.data.Model',
    fields: ['WAERS', 'LTEXT']
});

Ext.define('AccountModel', {
    extend: 'Ext.data.Model',
    fields: ['TUSER', 'UNA', 'EKGRP', 'DEPT_NO']
});

Ext.getUrlParam = function (param) {
    var params = Ext.urlDecode(location.search.substring(1));
    return param ? params[param] : params;
};
var mlifnr = Ext.getUrlParam('mlifnr');
var code = Ext.getUrlParam('code');
var edit = Ext.getUrlParam('edit');
//var tmpkey = Ext.getUrlParam('key');

var fieldStore = Ext.create('Ext.data.Store', {
    model: 'fieldModel',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'VID', direction: 'ASC' }],

    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        },
        load: function (store, records, successful, eOpts) {
            Ext.getCmp('KONZS1').setValue(records[0].data['KONZS']);
            //Ext.getCmp('stceg').setValue(records[0].data['STCEG']);
            //Ext.getCmp('name1').setValue(records[0].data['NAME1']);
            //Ext.getCmp('name2').setValue(records[0].data['NAME2']);
            //Ext.getCmp('pstlz').setValue(records[0].data['PSTLZ']);
            //Ext.getCmp('ort01').setValue(records[0].data['ORT01']);
            //Ext.getCmp('stras').setValue(records[0].data['STRAS']);
            //Ext.getCmp('land1').setValue(records[0].data['LAND1']);
            //alert(Ext.getCmp('SP01_detail'));
            Ext.getCmp('SP01_detail').loadRecord(records[0]);
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

// 聯絡人清單
var T1Store = Ext.create('Ext.data.Store', {
    model: 'T1Model',
    pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'VID5', direction: 'ASC' }],

    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/QueryContact',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 可報價物料清單
var T2Store = Ext.create('Ext.data.Store', {
    model: 'T2Model',
    pageSize: 15,
    remoteSort: true,
    sorters: [{ property: 'LTSNR', direction: 'ASC' }],

    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/MM/QueryMVM',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 供應商代碼 123類
//var T3Store = Ext.create('Ext.data.Store', {
//    model: 'T3Model',
//    //pageSize: 20,
//    remoteSort: true,
//    sorters: [{ property: 'VID1', direction: 'ASC' }],

//    listeners: {
//        beforeload: function (store, options) {
//            var np = {
//                //p0: T1Query.getForm().findField('P0').getValue(),
//                p1: mlifnr
//            };
//            Ext.apply(store.proxy.extraParams, np);
//        },
//        load: function (store, records, successful, eOpts) {
//            for (i = 0; i < records.length; i++) {
//                if (records[i].data['LIFNR'].substring(0, 1) == '1') {
//                    Ext.getCmp('LIFNR1').setValue(records[i].data['LIFNR']);
//                    Ext.getCmp("chk1").setValue(true);  
//                }
//                else if (records[i].data['LIFNR'].substring(0, 1) == '2') {
//                    Ext.getCmp('LIFNR2').setValue(records[i].data['LIFNR']);
//                    Ext.getCmp("chk2").setValue(true);
//                }
//                else {
//                    Ext.getCmp('LIFNR3').setValue(records[i].data['LIFNR']);
//                    Ext.getCmp("chk3").setValue(true);
//                }

//            }

//        }
//    },

//    proxy: {
//        type: 'ajax',
//        actionMethods: 'POST',
//        url: '../../../api/SP/QueryLifnr123',
//        reader: {
//            type: 'json',
//            root: 'etts',
//            totalProperty: 'rc'
//        }
//    }
//});
var T3Store = Ext.create('Ext.data.Store', {
    model: 'T3Model',
    remoteSort: true,
    sorters: [{ property: 'VID1', direction: 'ASC' }],
    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: Ext.getCmp('TMPKEY').getValue()
                p0: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/QueryTmpKtokk',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var T4Store = Ext.create('Ext.data.Store', {
    model: 'T4Model',
    //pageSize: 20,
    remoteSort: true,
    sorters: [{ property: 'LAND1', direction: 'ASC' }],

    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/GetLand1Combo',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 幣別
//var T4Store = Ext.create('Ext.data.Store', {
//    model: 'T4Model',
//    remoteSort: true,
//    sorters: [{ property: 'WAERS', direction: 'ASC' }],
//    proxy: {
//        type: 'ajax',
//        actionMethods: 'POST',
//        url: '../../../api/NM/GetCurrency',
//        reader: {
//            type: 'json',
//            root: 'etts',
//            totalProperty: 'rc'
//        }
//    }
//});

// 付款方式
var T5Store = Ext.create('Ext.data.Store', {
    model: 'T5Model',
    remoteSort: true,
    sorters: [{ property: 'ZLSCH', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetZwels',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 國貿條件
var T6Store = Ext.create('Ext.data.Store', {
    model: 'T6Model',
    //remoteSort: true,
    //sorters: [{ property: 'INCO1', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetInco1',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 付款條件
var T7Store = Ext.create('Ext.data.Store', {
    model: 'T7Model',
    //remoteSort: true,
    //sorters: [{ property: 'INCO1', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetZterm',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var T8Store = Ext.create('Ext.data.Store', {
    model: 'T8Model',
    remoteSort: true,
    sorters: [{ property: 'VID', direction: 'ASC' }],
    listeners: {
        beforeload: function (store, options) {
            var np = {
                //p0: Ext.getCmp('TMPKEY').getValue()
                p0: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/SP/QueryRecord',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 部門
var T9Store = Ext.create('Ext.data.Store', {
    model: 'T9Model',
    //remoteSort: true,
    //sorters: [{ property: 'INCO1', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetABTNR',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 功能 職稱
var T10Store = Ext.create('Ext.data.Store', {
    model: 'T10Model',
    //remoteSort: true,
    //sorters: [{ property: 'INCO1', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetPAFKT',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

// 幣別
var T11Store = Ext.create('Ext.data.Store', {
    model: 'T11Model',
    remoteSort: true,
    sorters: [{ property: 'WAERS', direction: 'ASC' }],
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetCurrency',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var ktokkStore = Ext.create('Ext.data.Store', {
    //fields: ['value', 'name'],
    //data: [
    //    { "value": "1ROH", "name": "1原料" },
    //    { "value": "2MAT", "name": "2包材" },
    //    { "value": "3NOR", "name": "3一般物品" }
    //]
    fields: ['KTOKK', 'TEXT1'],
    listeners: {
        beforeload: function (store, options) {
            var np = {
                p0: mlifnr
            };
            Ext.apply(store.proxy.extraParams, np);
        }
    },
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/NM/GetKTOKK',
        reader: {
            type: 'json',
            root: 'etts',
            totalProperty: 'rc'
        }
    }
});

var ynStore = Ext.create('Ext.data.Store', {
    fields: ['value', 'name'],
    data: [
        { "value": "", "name": "" },
        { "value": "Y", "name": "Y" },
    ]
});

var vipStore = Ext.create('Ext.data.Store', {
    fields: ['value', 'name'],
    data: [
        { "value": "", "name": "" },
        { "value": "1", "name": "1" },
    ]
});

var AccountStore = Ext.create('Ext.data.Store', {
    model: 'AccountModel',
    proxy: {
        type: 'ajax',
        actionMethods: 'POST',
        url: '../../../api/Acct/Info',
        reader: {
            type: 'json',
            root: 'etts'
        }
    },
    listeners: {
        load: function (sender, records, successful, eOpts) {
            if (successful) {
                var userID = records[0].get('TUSER');
                Ext.getCmp('LOGIN_USERID').setValue(userID);
            }
        }
    }
});

