Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isARMY = "N";
    isARMY = Ext.getUrlParam('isARMY');
    debugger
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '/api/AB0104/Excel';
    var mLabelWidth = 80;
    var mWidth = 200;
    var radioSelected = 'isCombo';
    var isRadioChanged = false;

    var towhStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var frwhStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P2',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 180,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0104/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var strWH_NO = '';
            if (T1Query.getForm().findField('rd1').checked) {
                strWH_NO = T1Query.getForm().findField('P0').getValue();
            } else {
                strWH_NO = T1Query.getForm().findField('P0X').getValue();
            }

            return {
                wh_no: strWH_NO
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("P2").setValue(r.get('MMCODE'));
                }
            }
        }
    });
    var flowidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function getTowhCombo() {
        Ext.Ajax.request({
            url: '/api/AB0104/GetTowhCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var towhs = data.etts;
                    if (towhs.length > 0) {
                        towhStore.add({
                            VALUE: '',
                            TEXT: ''
                        });
                        for (var i = 0; i < towhs.length; i++) {
                            towhStore.add({
                                VALUE: towhs[i].VALUE,
                                TEXT: towhs[i].TEXT
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTowhCombo();
    function getFrwhCombo() {
        Ext.Ajax.request({
            url: '/api/AB0104/GetFrwhCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var frwhs = data.etts;
                    if (frwhs.length > 0) {
                        frwhStore.add({
                            VALUE: '',
                            TEXT: ''
                        });
                        for (var i = 0; i < frwhs.length; i++) {
                            frwhStore.add({
                                VALUE: frwhs[i].VALUE,
                                TEXT: frwhs[i].TEXT
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getFrwhCombo();
    function getFlowidCombo() {
        Ext.Ajax.request({
            url: '/api/AB0104/GetFlowidCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var flowids = data.etts;
                    if (flowids.length > 0) {
                        flowidStore.add({
                            VALUE: '',
                            TEXT: ''
                        });
                        for (var i = 0; i < flowids.length; i++) {
                            flowidStore.add({
                                VALUE: flowids[i].VALUE,
                                TEXT: flowids[i].TEXT
                            });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getFlowidCombo();

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'DATA_DATE',
            'DATA_BTIME',
            'DATA_ETIME',
            'WH_NO',
            'MMCODE',
            'VISIT_KIND',
            'CONSUME_QTY',
            'STOCK_UNIT',
            'INSU_QTY',
            'HOSP_QTY',
            'PARENT_ORDERCODE',
            'PARENT_CONSUME_QTY',
            'CREATEDATETIME',
            'PROC_ID',
            'PROC_MSG',
            'PROC_TYPE'
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AB.AB0104', {
        pageSize: 50, // 每頁顯示筆數
        listeners: {
            beforeload: function (store, options) {
                var strTOWH = '', strFRWH = '';

                if (T1Query.getForm().findField('rd1').checked) {
                    strTOWH = T1Query.getForm().findField('P0').getValue();
                    strFRWH = T1Query.getForm().findField('P1').getValue();
                } else {
                    strTOWH = T1Query.getForm().findField('P1X').getValue();
                    strFRWH = T1Query.getForm().findField('P0X').getValue();
                }
                var np = {
                    towh: strTOWH,
                    frwh: strFRWH,
                    start_date: T1Query.getForm().findField('P3').rawValue,
                    end_date: T1Query.getForm().findField('P4').rawValue,
                    flowid: T1Query.getForm().findField('P5').getValue(),
                    is_ab: T1Query.getForm().findField('P6').checked ? 'Y' : 'N',
                    mmcode: T1Query.getForm().findField('P2').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

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
                    xtype: 'radio',
                    name: 'rd1',
                    id: 'rd1',
                    width: 10,
                    checked: true,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            T1Query.getForm().findField('P0').enable();
                            T1Query.getForm().findField('P1').enable();
                            T1Query.getForm().findField('P0X').disable();
                            T1Query.getForm().findField('P1X').disable();
                            T1Query.getForm().findField('rd2').setValue(false);
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: towhStore,
                    name: 'P0',
                    id: 'P0',
                    fieldLabel: '調入庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'combo',
                    store: frwhStore,
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 80,
                    width: 200,
                    fieldLabel: '調出庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                mmCodeCombo,
                {
                    xtype: 'datefield',
                    fieldLabel: '資料日期',
                    name: 'P3',
                    id: 'P3',
                    width: 160,
                    labelWidth: mLabelWidth,
                    allowBlank: true,
                    value: new Date((new Date()).getFullYear(), (new Date()).getMonth(), 1),
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P4',
                    id: 'P4',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 98,
                    allowBlank: true,
                    padding: '0 4 0 4',
                    value: new Date(),
                },
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'radio',
                    name: 'rd2',
                    id: 'rd2',
                    width: 10,
                    listeners: {
                        change: function (field, newValue, oldValue) {
                            T1Query.getForm().findField('P0').disable();
                            T1Query.getForm().findField('P1').disable();
                            T1Query.getForm().findField('P0X').enable();
                            T1Query.getForm().findField('P1X').enable();
                            T1Query.getForm().findField('rd1').setValue(false);
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: towhStore,
                    name: 'P0X',
                    id: 'P0X',
                    fieldLabel: '調出庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    disabled: true
                },
                {
                    xtype: 'combo',
                    store: frwhStore,
                    name: 'P1X',
                    id: 'P1X',
                    labelWidth: 80,
                    width: 200,
                    fieldLabel: '調入庫房',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    disabled: true
                },
                {
                    xtype: 'combo',
                    store: flowidStore,
                    name: 'P5',
                    id: 'P5',
                    labelWidth: 80,
                    width: 200,
                    fieldLabel: '申請單狀態',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'fieldcontainer',
                    fieldLabel: '異常',
                    defaultType: 'checkboxfield',
                    labelWidth: 110,
                    labelSeparator: '',
                    width: 150,
                    items: [
                        {
                            name: 'P6',
                            inputValue: '1',
                            checked: true
                        }
                    ]
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        if (T1Query.getForm().findField('rd1').checked) {
                            if (!T1Query.getForm().findField('P0').getValue()) {
                                Ext.Msg.alert('提醒', '<span style="color:red">調入庫房</span>為必填');
                                return;
                            }
                        } else {
                            if (!T1Query.getForm().findField('P0X').getValue()) {
                                Ext.Msg.alert('提醒', '<span style="color:red">調出庫房</span>為必填');
                                return;
                            }
                        }
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        f.reset();

                        msglabel('訊息區:');
                    }
                }
            ]
        }
        ]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {

                    if (T1Query.getForm().findField('rd1').checked) {
                        if (!T1Query.getForm().findField('P0').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">調入庫房</span>為必填');
                            return;
                        }
                    } else {
                        if (!T1Query.getForm().findField('P0X').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">調出庫房</span>為必填');
                            return;
                        }
                    }
                    var strTOWH = '', strFRWH = '';

                    if (T1Query.getForm().findField('rd1').checked) {
                        strTOWH = T1Query.getForm().findField('P0').getValue();
                        strFRWH = T1Query.getForm().findField('P1').getValue();
                    } else {
                        strTOWH = T1Query.getForm().findField('P1X').getValue();
                        strFRWH = T1Query.getForm().findField('P0X').getValue();
                    }
                    var p = new Array();
                    p.push({ name: 'FN', value: '調撥資料查詢.xls' });
                    p.push({ name: 'towh', value: strTOWH });
                    p.push({ name: 'frwh', value: strFRWH });
                    p.push({ name: 'start_date', value: T1Query.getForm().findField('P3').rawValue });
                    p.push({ name: 'end_date', value: T1Query.getForm().findField('P4').rawValue });
                    p.push({ name: 'flowid', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'is_ab', value: T1Query.getForm().findField('P6').checked ? 'Y' : 'N' });
                    p.push({ name: 'mmcode', value: T1Query.getForm().findField('P2').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "調入庫房",
            dataIndex: 'TOWH',
            width: 80
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 120
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        }, {
            text: "申請日期",
            dataIndex: 'APPTIME',
            width: 80
        }, {
            text: "申請人員",
            dataIndex: 'APPID',
            width: 70
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: '調出庫房',
            dataIndex: 'FRWH',
            width: 80
        }, {
            text: "調出日期",
            dataIndex: 'APVTIME',
            width: 80
        }, {
            text: "核撥人員",
            dataIndex: 'APVID',
            width: 70
        }, {
            text: "調出數量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            align: 'right',
            width: 70
        }, {
            text: "調入日期",
            dataIndex: 'ACKTIME',
            width: 80
        }, {
            text: "點收人員",
            dataIndex: 'ACKID',
            width: 80
        }, {
            text: "調入數量",
            dataIndex: 'ACKQTY',
            width: 70,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "實際調入數量",
            dataIndex: 'RCVQTY',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "調撥短少數量",
            dataIndex: 'TRNAB_QTY',
            width: 100,
            style: 'text-align:left',
            align: 'right',
        }, {
            text: "調撥異常原因",
            dataIndex: 'TRNAB_RESON',
            width: 100
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 100
        }, {
            text: "申請單狀態",
            dataIndex: 'FLOWID',
            width: 80
        }, {
            text: "申請部門",
            dataIndex: 'APPDEPT',
            width: 100
        }, {
            header: "",
            flex: 1
        }]
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    //#region viewport
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
            items: [T1Grid]
        }
        ]
    });
    //#endregion

    if (isARMY == 'Y') {
        T1Query.getForm().findField('P6').setValue(false);
        T1Grid.down('#export').setDisabled(false);
    }
    T1Grid.down('#export').setDisabled(false);
});