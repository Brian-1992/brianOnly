Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1GetExcel = '/api/AB0101/Excel';
    var ordersortComboGet = '/api/AB0101/GetOrdersortCombo';
    var mLabelWidth = 60;
    var mWidth = 170;
    var radioSelected = 'isCombo';
    var isRadioChanged = false;
    
    var frwhCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P2_Combo',
        width: 100,
        allowBlank: true,
        labelWidth: 51,
        padding:' 0 0 0 4',
        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            
            return {
                //WH_NO: tmpArray[0]
                //p1: T1Query.getForm().findField('GRP_NO').getValue(),
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0101/FrwhNos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));


                var f = T1Query.getForm();
                if (r.get('WH_NO') !== '') {
                    f.findField("P2_Combo").setValue(r.get('WH_NO'));
                }
            },
            change: function (field, newValue, oldValue) {
            }
        }
    });
    var towhCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P3',
        width: mWidth,
        allowBlank: true,
        fieldLabel: '繳回庫房',
        labelWidth: mLabelWidth,
        padding: ' 0 0 0 4',
        labelWidth:60, 
        //限制一次最多顯示10筆
        limit: 10,
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            return {
            };
        },
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0101/TowhNos',
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));


                var f = T1Query.getForm();
                if (r.get('WH_NO') !== '') {
                    f.findField("P3").setValue(r.get('WH_NO'));
                }
            },
            change: function (field, newValue, oldValue) {
            }
        }
    });
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P4',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 158,
        labelWidth: 50,
        //限制一次最多顯示10筆
        limit: 10,
        padding: '0 0 0 4',
        //指定查詢的Controller路徑
        queryUrl: '/api/AB0101/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("P4").setValue(r.get('MMCODE'));
                }
            }
        }
    });

    // 過帳分類
    var postTypeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { 'VALUE': '0', 'TEXT': '手動點收'},
            { 'VALUE': '1', 'TEXT': '系統點收' },
            { 'VALUE': '2', 'TEXT': '全部' }
        ]
    });

    var ordersortQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: ordersortComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        ordersortQueryStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            ordersortQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        
    }
    setComboData();

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            'DOCNO',
            'FLOWID',
            'APPTIME',
            'FRWH',
            'TOWH',
            'MMCODE',
            'MMNAME_E',
            'MMNAME_C',
            'BASE_UNIT',
            'STAT',
            'APVQTY',
            'BACKQTY',
            'SEQ',
            'POSTTYPE'
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: radioSelected == 'isCombo' ?
                        T1Query.getForm().findField('P2_Combo').getValue() :
                        T1Query.getForm().findField('P2_Text').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: '/api/AB0101/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
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
                    xtype: 'datefield',
                    fieldLabel: '退藥日期',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: mLabelWidth,
                    allowBlank: true,
                    value: new Date((new Date()).getFullYear(), (new Date()).getMonth(), 1),
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 8,
                    labelSeperator: '',
                    width: 118,
                    allowBlank: true,
                    padding: '0 4 0 4',
                    value: new Date(),
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '退藥庫房(病房)',
                    labelWidth: 100,
                    width: 100
                },
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P2',
                    inputValue: 'dropdown',
                    id: 'radio1',
                    padding: '0 0 0 4',
                    width: 15,
                    checked: true,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isCombo';
                                T1Query.getForm().findField('P2_Combo').enable();
                                T1Query.getForm().findField('P2_Text').disable();
                            }
                        }
                    }
                },
                frwhCombo,
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P2',
                    inputValue: 'textfield',
                    id: 'radio2',
                    padding: '0 0 0 4',
                    width: 15,
                    checked: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isTextfield';
                                T1Query.getForm().findField('P2_Combo').disable();
                                T1Query.getForm().findField('P2_Text').enable();
                            }
                        }
                    }
                },
                {
                    xtype: 'textfield',
                    name: 'P2_Text',
                    labelField: '',
                    width: '90',
                    emptyText: '自行輸入',
                    disabled: true
                },

            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                towhCombo,
                mmCodeCombo,
                {
                    xtype: 'combo',
                    store: postTypeQueryStore,
                    id: 'P5',
                    name: 'P5',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '過帳分類',
                    queryMode: 'local',
                    width: 183,
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                }, {
                    xtype: 'checkbox',
                    id: 'P6',
                    name: 'P6',
                    margin: '0 0 0 4',
                    boxLabel: '有差異',
                    width: 70,
                    value: true
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();

                        vChkYM = "";
                        f.reset();

                        f.findField('CHK_YM').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '退藥差異量總表.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({
                        name: 'p2', value: radioSelected == 'isCombo' ?
                                           T1Query.getForm().findField('P2_Combo').getValue() :
                                           T1Query.getForm().findField('P2_Text').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            },
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
            text: "退藥單號",
            dataIndex: 'DOCNO',
            width: 100
        }, {
            text: "單據狀態",
            dataIndex: 'FLOWID',
            width: 90
        }, {
            text: "退藥日期",
            dataIndex: 'APPTIME',
            width: 80
        }, {
            text: "退藥庫房(病房)",
            dataIndex: 'FRWH',
            width: 120
        }, {
            text: "繳回庫房",
            dataIndex: 'TOWH',
            width: 120
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 120
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120
        }, {
            text: '<span style="color:blue">HIS退藥量</span>',
            dataIndex: 'BACKQTY',
            style: 'text-align:left',
            align: 'right',
            width: 95,
            renderer: function (val, meta, record) {
                var BACKQTY = record.data.BACKQTY;
                return '<a href=javascript:void(0)>' + BACKQTY + '</a>';
            },
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 80
        }, {
            text: "狀態",
            dataIndex: 'STAT',
            width: 80
        }, {
            text: "實際退藥量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "過帳分類",
            dataIndex: 'POSTTYPE',
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'BACKQTY');
                if (index != cellIndex) {
                    return;
                }

                T3Store.removeAll();
                T3Query.getForm().findField('P30').setValue('');
                T3Load(record.data.DOCNO, record.data.SEQ, '');
                window.show();
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    //#region 病患資料
    var viewModel = Ext.create('WEBAPP.store.AB.AB0023VM');
    var T3Store = viewModel.getStore('ME_BACK');
    function T3Load(docno, seq, ordersort) {
        T3Store.getProxy().setExtraParam("p0", docno);
        T3Store.getProxy().setExtraParam("p1", seq);
        T3Store.getProxy().setExtraParam("p2", ordersort);
        T3Tool.moveFirst();
    }

    var T3Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP31',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: ordersortQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    id: 'P30',
                    name: 'P30',
                    fieldLabel: '醫令種類',
                    queryMode: 'local',
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                }, {
                    xtype: 'button', 
                    text: '查詢',
                    margin: '0 5 0 20',
                    handler: function () {
                        T3Store.getProxy().setExtraParam("p2", this.up('form').getForm().findField('P30').getValue());
                        T3Tool.moveFirst();
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
        }]
    });

    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        border: false,
        plain: true,
    });

    var T3Grid = Ext.create('Ext.grid.Panel', {
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T3Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [
            {
                text: "病房",
                dataIndex: 'NRCODE',
                width: 50
            },
            {
                text: "病床號",
                dataIndex: 'BEDNO',
                width: 50
            },
            {
                text: "病歷號",
                dataIndex: 'CHARTNO',
                width: 80
            },
            {
                text: "姓名",
                dataIndex: 'CHINNAME',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'ORDERCODE',
                width: 100
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME',
                width: 300
            },
            {
                text: "開立日期",
                dataIndex: 'BEGINDATETIME',
                width: 100
            },
            {
                text: "DC 日期",
                dataIndex: 'ENDDATETIME',
                width: 100
            },
            {
                text: "劑量",
                dataIndex: 'DOSE',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "頻率",
                dataIndex: 'FREQNO',
                width: 50
            },
            {
                text: "建議退量",
                dataIndex: 'NEEDBACKQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "實際退量",
                dataIndex: 'BACKQTY',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "差異量",
                dataIndex: 'DIFF',
                width: 70,
                align: 'right',
                style: 'text-align:left',
            },
            {
                text: "修改原因",
                dataIndex: 'PHRBACKREASON_NAME',
                width: 100
            },
            {
                text: "退藥日期",
                dataIndex: 'CREATEDATETIME',
                width: 100
            },
            {
                text: "醫令種類",
                dataIndex: 'ORDERSORT',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        //plugins: [
        //    Ext.create('Ext.grid.plugin.CellEditing', {
        //        clicksToEdit: 1
        //    })
        //],
        listeners: {
            selectionchange: function (model, records) {
                msglabel('訊息區:');
                Ext.getCmp('eastform').expand();
            }
        }
    });

    var window = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        items: [T3Grid],
        width: "600px",
        height: "300px",
        resizable: true,
        modal: true,
        title: "病患資料",
        layout: 'fit',
        buttons: [{
            text: '關閉',
            handler: function () {
                this.up('window').hide();
            }
        }]
    });
    window.hide();

    //#endregion


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

    T1Query.getForm().findField('P5').setValue('0');
});