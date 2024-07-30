Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0044/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "庫存基本檔查詢";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
   
    var user_kind = '';
    var wh_no = '';
    var mm_code = '';
    var winAA0129;

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');


    function getUserKind() {
        Ext.Ajax.request({
            url: '/api/AA0129/GetUserkind',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_kind = data;
            },
            failure: function (response, options) {

            }
        });
    }
    getUserKind();
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_setym = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/GetSetymCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_whkind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/GetWhkindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });
    var st_whgrade = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/GetWhgradeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        autoLoad: true
    });

    var wh_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0044/GetWhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: user_kind,
                    menuLink: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '', COMBITEM: '' });
            }
        },
        autoLoad: true
    });

    var mat_class_store = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0129/GetMAT_CLASSCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                var wh_kind = '';
                var index = wh_store.find('VALUE', T1Query.getForm().findField('P3').getValue());
                if (index != -1)
                    wh_kind = wh_store.data.items[wh_store.find('VALUE', T1Query.getForm().findField('P3').getValue())].data.COMBITEM
                var np = {
                    p0: wh_kind,
                    user_kind: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });
    var st_storeid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/GetStoreidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },listeners: {
            beforeload: function (store, options) {
                var wh_kind = '';
                var index = wh_store.find('VALUE', T1Query.getForm().findField('P0').getValue());
                if (index != -1)
                    wh_kind = wh_store.data.items[wh_store.find('VALUE', T1Query.getForm().findField('P0').getValue())].data.COMBITEM
                var np = {
                    p0: wh_kind, 
                    user_kind: user_kind
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store) {
                store.insert(0, { TEXT: '', VALUE: '' });
            }
        },
        autoLoad: true
    });
    

    var mLabelWidth = 90;
    var mWidth = 230;
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P6',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: mLabelWidth,
        width: mWidth,
        padding: '0 4 0 4',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0044/GetMMCodeDocd', //指定查詢的Controller路徑
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P4').getValue(),
                m_stroeid: T1Query.getForm().findField('P5').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '年月',
                    name: 'P0',
                    id: 'P0',
                    store: st_setym,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'hidden',
                    fieldLabel: '庫別',
                    name: 'P1',
                    id: 'P1'
                    //,
                    //store: st_whkind,
                    //queryMode: 'local',
                    //displayField: 'TEXT',
                    //valueField: 'VALUE',
                    ////allowBlank: false, // 欄位為必填
                    //fieldCls: 'required',
                    //labelAlign: 'right',
                    //labelWidth: mLabelWidth,
                    //width: mWidth,
                    //padding: '0 4 0 4',
                    //listeners: {
                    //    "select": function (combobox, records, eOpts) {
                    //        st_whgrade.load();
                    //        wh_store.load();
                    //    }, boxready: function () {
                    //        //this.store.load();
                    //    }
                    //}
                },
                {
                    xtype: 'hidden',
                    fieldLabel: '庫房級別',
                    name: 'P2',
                    id: 'P2'
                    //,
                    //store: st_whgrade,
                    //queryMode: 'local',
                    //displayField: 'TEXT',
                    //valueField: 'VALUE',
                    //labelAlign: 'right',
                    //labelWidth: mLabelWidth,
                    //width: mWidth,
                    //padding: '0 4 0 4',
                    //listeners: {
                    //    "select": function (combobox, records, eOpts) {
                    //        wh_store.load();
                    //    }, boxready: function () {
                    //        //this.store.load();
                    //    }
                    //}
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫房',
                    id: 'P3',
                    queryMode: 'local',
                    store: wh_store,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    matchFieldWidth: false,
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    width: mWidth,
                    padding: '0 4 0 4',
                    listConfig: {
                        width: 230
                    },
                    listeners: {
                        select: function (combo, record, index) {
                            if (user_kind == "S") {
                                Ext.getCmp('P4').setValue("");
                                Ext.getCmp('P4').setRawValue("");
                                mat_class_store.load();
                            }
                        }
                    }
                }, 
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    id: 'P4',
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    queryMode: 'local',
                    store: mat_class_store,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'hidden',
                    fieldLabel: '庫備識別',
                    name: 'P5',
                    id: 'P5'
                    //,
                    //store: st_storeid,
                    //queryMode: 'local',
                    //displayField: 'COMBITEM',
                    //valueField: 'VALUE',
                    //labelAlign: 'right',
                    //labelWidth: mLabelWidth,
                    //width: mWidth,
                    //padding: '0 4 0 4'
                }, T1QueryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null) 
                        ) {
                            Ext.Msg.alert('提醒', '年月不可空白');
                        }
                        else {
                            T1Load();
                            msglabel('訊息區:');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'INV_QTY_N', type: 'string' },//結存
            { name: 'INV_QTY_L', type: 'string' },
            { name: 'APL_INQTY_N', type: 'string' },//進貨
            { name: 'APL_INQTY_L', type: 'string' },
            { name: 'APL_OUTQTY_N', type: 'string' },//撥發
            { name: 'APL_OUTQTY_L', type: 'string' },
            { name: 'TRN_INQTY_N', type: 'string' },//調撥入庫
            { name: 'TRN_INQTY_L', type: 'string' },
            { name: 'TRN_OUTQTY_N', type: 'string' },//調撥出庫
            { name: 'TRN_OUTQTY_L', type: 'string' },
            { name: 'ADJ_INQTY_N', type: 'string' },//調帳入庫
            { name: 'ADJ_INQTY_L', type: 'string' },
            { name: 'ADJ_OUTQTY_N', type: 'string' },//調帳出庫
            { name: 'ADJ_OUTQTY_L', type: 'string' },
            { name: 'BAK_INQTY_N', type: 'string' },//繳回入庫
            { name: 'BAK_INQTY_L', type: 'string' },
            { name: 'BAK_OUTQTY_N', type: 'string' },//繳回出庫
            { name: 'BAK_OUTQTY_L', type: 'string' },
            { name: 'EXG_INQTY_N', type: 'string' },//換貨入庫
            { name: 'EXG_INQTY_L', type: 'string' },
            { name: 'EXG_OUTQTY_N', type: 'string' },//換貨出庫
            { name: 'EXG_OUTQTY_L', type: 'string' },
            { name: 'MIL_INQTY_N', type: 'string' },//戰備入庫
            { name: 'MIL_INQTY_L', type: 'string' },
            { name: 'MIL_OUTQTY_N', type: 'string' },//戰備出庫
            { name: 'MIL_OUTQTY_L', type: 'string' },
            { name: 'ONWAY_QTY_N', type: 'string' },//在途入庫
            { name: 'ONWAY_QTY_L', type: 'string' },
            { name: 'REJ_OUTQTY_N', type: 'string' },//退貨出庫
            { name: 'REJ_OUTQTY_L', type: 'string' },
            { name: 'DIS_OUTQTY_N', type: 'string' },//報廢出庫
            { name: 'DIS_OUTQTY_L', type: 'string' },
            { name: 'INVENTORYQTY1_N', type: 'string' },//盤盈入庫
            { name: 'INVENTORYQTY1_L', type: 'string' },
            { name: 'INVENTORYQTY2_N', type: 'string' },//盤虧出庫
            { name: 'INVENTORYQTY2_L', type: 'string' },
            { name: 'USE_QTY_N', type: 'string' },//耗用量
            { name: 'USE_QTY_L', type: 'string' },
            { name: 'DATA_YM_N', type: 'string' },
            { name: 'DATA_YM_L', type: 'string' },
            { name: 'TOTQTY_N', type: 'string' },
            { name: 'TOTQTY_L', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    menuLink: menuLink
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        autoScroll: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        },{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        } ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "上期結存",
            dataIndex: 'INV_QTY_L',
            style: 'text-align:left',
            width: 100, align: 'right',
            //renderer: function (val, meta, record) {
            //    return val + "<br />" + record.get('INV_QTY_N');
            //}
            }, {
                text: "進貨入庫<br />撥發出庫",
                dataIndex: 'APL_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","APLI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","APLO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('APL_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('APL_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('APL_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('APL_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('APL_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('APL_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
                
            }, {
                text: "調撥入庫<br />調撥出庫",
                dataIndex: 'TRN_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","TRNI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","TRNO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('TRN_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('TRN_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('TRN_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('TRN_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('TRN_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('TRN_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
            }, {
                text: "調帳入庫<br />調帳出庫",
                dataIndex: 'ADJ_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","ADJI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","ADJO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('ADJ_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('ADJ_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('ADJ_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('ADJ_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('ADJ_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('ADJ_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
            }, {
                text: "繳回入庫<br />繳回出庫",
                dataIndex: 'BAK_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","BAKI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","BAKO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('BAK_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('BAK_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('BAK_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('BAK_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('BAK_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('BAK_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
            }, {
                text: "換貨入庫<br />換貨出庫",
                dataIndex: 'EXG_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","EXGI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","EXGO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('EXG_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('EXG_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('EXG_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('EXG_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('EXG_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('EXG_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
            }, {
                text: "戰備入庫<br />戰備出庫",
                dataIndex: 'MIL_INQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","MILI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","MILO","' + record.get('DATA_YM_N') + '");';
                    if (val == 0) {
                        if (record.get('MIL_OUTQTY_N') == 0) {
                            rtn = val + "<br />" + record.get('MIL_OUTQTY_N');
                        }
                        else {
                            rtn = val + "<br />" + "<a href=" + hrefn + ">" + record.get('MIL_OUTQTY_N') + "</a>";
                        }
                    }
                    else {
                        if (record.get('MIL_OUTQTY_N') == 0) {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + record.get('MIL_OUTQTY_N');
                        }
                        else {
                            rtn = "<a href=" + hrefl + ">" + val + "</a>" + "<br />" + "<a href=" + hrefn + ">" + record.get('MIL_OUTQTY_N') + "</a>";
                        }
                    }
                    return rtn;
                }
            },  {
                text: "在途入庫<br />退貨出庫",
                dataIndex: 'ONWAY_QTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","WAYI","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","REJO","' + record.get('DATA_YM_N') + '");';
                    if (record.get('REJ_OUTQTY_N') == 0) {
                        rtn = "0<br />" + record.get('REJ_OUTQTY_N');
                    }
                    else {
                        rtn = "0<br />" + "<a href=" + hrefn + ">" + record.get('REJ_OUTQTY_N') + "</a>";
                    }

                    return rtn;
                }
            },  {
                text: "報廢入庫<br />報廢出庫",
                dataIndex: 'DIS_OUTQTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    var rtn = "";
                    var hrefl = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","DISO","' + record.get('DATA_YM_N') + '");';
                    var hrefn = 'javascript:T2Load("' + record.get('WH_NO') + '","' + record.get('MMCODE') + '","DISO","' + record.get('DATA_YM_N') + '");';
                    rtn = "0<br />" + "<a href=" + hrefn + ">" + val + "</a>";
                    return rtn;
                }
            }, {
                text: "盤盈<br />盤虧",
                dataIndex: 'INVENTORYQTY1_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                renderer: function (val, meta, record) {
                    return val + "<br />" + record.get('INVENTORYQTY2_N');
                }
            }, {
                text: "耗用量",
                dataIndex: 'USE_QTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                //renderer: function (val, meta, record) {
                //    return "0<br />" + val;
                //}
            }, {
                text: "本期結存",
                dataIndex: 'INV_QTY_N',
                style: 'text-align:left',
                width: 100, align: 'right',
                //renderer: function (val, meta, record) {
                //    return val + "<br />" + record.get('TOTQTY_L');
                //}
            }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    
                }
            }
        }
    });
    T2Load = function (whno, mmcode, mcode, dataymn) {
        T2WH_NO = whno;
        T2MMCODE = mmcode;
        T2MCODE = mcode;
        T2DATA_YM = dataymn; 
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
            T2Tool.moveFirst();
            showWin2();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        
    }

    function setFormT1a() {
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        //viewport.down('#form').expand();
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('WH_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T1F1 = f.findField('WH_NO').getValue();
            T1F2 = f.findField('MMCODE').getValue();

        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
            T1F2 = '';
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCTYPE',
            xtype: 'hidden'
        }, {
            name: 'FLOWID',
            xtype: 'hidden'
        }, {
            name: 'FRWH',
            xtype: 'hidden'
        }, {
            name: 'TOWH',
            xtype: 'hidden'
        }, {
            name: 'APPLY_KIND',
            xtype: 'hidden'
        }, {
            name: 'MAT_CLASS',
            xtype: 'hidden'
        }, {
            name: 'APPID',
            xtype: 'hidden'
        }, {
            name: 'APPDEPT',
            xtype: 'hidden'
        }, {
            name: 'USEID',
            xtype: 'hidden'
        }, {
            name: 'USEDEPT',
            xtype: 'hidden'
        }, {
            name: 'APPTIME_T',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請單號',
            name: 'DOCNO_D'
        }, {
            fieldLabel: '類別',
            name: 'APPLY_KIND_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'FLOWID_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請時間',
            name: 'APPTIME_T'
        }, {
            xtype: 'displayfield',
            fieldLabel: '出庫庫房',
            name: 'FRWH_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '入庫庫房',
            name: 'TOWH_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS_N'
        }, {
            xtype: 'displayfield',
            fieldLabel: '備註',
            name: 'APPLY_NOTE'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
                /*else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');*/
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            break;
                        case "A":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料核撥成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            break;
                    }
                    T1Cleanup();
                    T1Load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Query.getForm().findField('P1').setValue('1');
        //setFormT1a();
        T2Cleanup();
    }

    //Detail
    var T2WH_NO, T2MMCODE, T2MCODE, T2DATA_YM;
    var T2Rec = 0;
    var T2LastRec = null;
    

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'TR_DATE', type: 'string' },
            { name: 'TR_SEO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'TR_INV_QTY', type: 'string' },
            { name: 'TR_ONWAY_QTY', type: 'string' },
            { name: 'TR_DOCNO', type: 'string' },
            { name: 'TR_DOCSEQ', type: 'string' },
            { name: 'TR_FLOWID', type: 'string' },
            { name: 'TR_DOCTYPE', type: 'string' },
            { name: 'TR_IO', type: 'string' },
            { name: 'TR_MCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'TR_DOCTYPE_N', type: 'string' },
            { name: 'TR_MCODE_N', type: 'string' },
            { name: 'TR_IO_N', type: 'string' },
            { name: 'TR_DATE_T', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0044/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T2WH_NO,
                    p1: T2MMCODE,
                    p2: T2MCODE,
                    p3: T2DATA_YM
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'DOCNO',
            xtype: 'hidden'
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            name: 'STAT',
            xtype: 'hidden'
        }, {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '計量單位',
            name: 'BASE_UNIT'
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨單價',
            name: 'M_CONTPRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存單價',
            name: 'AVG_PRICE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '安全存量',
            name: 'SAFE_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存數量',
            name: 'INV_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '平均申請數量',
            name: 'AVG_APLQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請數量',
            name: 'APPQTY'
        }, {
            fieldLabel: '預計核撥量',
            name: 'EXPT_DISTQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true
        }, {
            fieldLabel: '調撥量',
            name: 'BW_MQTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            //fieldCls: 'required',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '實際核撥量',
            name: 'APVQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '核撥時間',
            name: 'APVTIME'
        }, {
            xtype: 'displayfield',
            fieldLabel: '月累計量',
            name: 'TOT_APVQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '累計調撥量',
            name: 'TOT_BWQTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '基準量',
            name: 'HIGH_QTY'
        }, {
            xtype: 'displayfield',
            fieldLabel: '撥發包裝率',
            name: 'TOT_DISTUN'
        }
        ],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if (this.up('form').getForm().findField('EXPT_DISTQTY').getValue() == '0')//|| this.up('form').getForm().findField('BW_MQTY').getValue() == '0')
                    Ext.Msg.alert('提醒', '預計核撥量及調撥量不可為0');
                else {
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T2Submit();
                        }
                    }
                    );
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: T2Cleanup
        }]
    });
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T11Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            r.commit();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            break;
                        case "D":
                            T2Store.remove(r);
                            r.commit();
                            break;
                    }
                    T2Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T2Name);
        //viewport.down('#form').expand();
        var f2 = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            f2.reset();
            //var r = Ext.create('T2Model');
            var r = Ext.create('WEBAPP.model.ME_DOCD');
            T2Form.loadRecord(r);
            f2.findField('DOCNO').setValue(T1F1);
            u = f2.findField("MMCODE");
            f2.findField('MMCODE').setReadOnly(false);
        }
        else {
            u = f2.findField('EXPT_DISTQTY');
            if (T2LastRec.get('EXPT_DISTQTY') == "" || T2LastRec.get('EXPT_DISTQTY') == "0") {
                f2.findField('EXPT_DISTQTY').setValue(T2LastRec.get('APPQTY'));
            }
        }

        f2.findField('x').setValue(x);
        f2.findField('STAT').setValue('1');
        f2.findField('EXPT_DISTQTY').setReadOnly(false);
        f2.findField('BW_MQTY').setReadOnly(false);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 150
        }, {
            text: "交易日期",
            dataIndex: 'TR_DATE_T',
            width: 80
        }, {
            text: "表單號碼",
            dataIndex: 'TR_DOCNO',
            width: 120
        }, {
            text: "異動數量",
            dataIndex: 'TR_INV_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "異動在途量",
            dataIndex: 'TR_ONWAY_QTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "出入庫識別",
            dataIndex: 'TR_IO_N',
            width: 100
        }, {
            text: "表單類別",
            dataIndex: 'TR_DOCTYPE_N',
            width: 120
        }, {
            text: "庫存異動",
            dataIndex: 'TR_MCODE_N',
            width: 100
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        }, {
            text: "合約單價",
            dataIndex: 'M_CONTPRICE',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            }
        }
    });
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
            if (T1F3 === '2') {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#delete').setDisabled(false);
            }
            else {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
            tot_apvqty = 0;
            apvqty = 0;
            appqty = 0;
            if (T2LastRec.get('TOT_APVQTY') == null || T2LastRec.get('TOT_APVQTY') == '') {
                apvqty = Number(0);
            }
            appqty = Number(T2LastRec.get('APPQTY'));
            tot_apvqty = Number(apvqty) + Number(appqty);
            f.findField('TOT_APVQTY').setValue(tot_apvqty);
        }
        else {
            T2Form.getForm().reset();
        }
    }

    //view 

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [
                {
                    itemId: 't1Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    items: [T1Grid]
                }
            ]
        }
        ]
    });

    var winActWidth = viewport.width - 10;
    var winActHeight = viewport.height - 10;
    var win2;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: '庫存明細',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T2Grid],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            },
            buttons: [{
                text: '關閉',
                handler: function () {
                    hideWin2();
                }
            }]
        });
    }
    function showWin2() {
        if (win2.hidden) {
            T2Cleanup();
            win2.show();
        }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
            T2Cleanup();
        }
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P1').setValue('1');
});
