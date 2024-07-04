Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

var agenComboGet = '../../../api/AA0085/GetAgenCombo';


// 廠商清單
var agen_noQueryStore = Ext.create('Ext.data.Store', {
    fields: ['VALUE', 'TEXT']
});

function setComboData() {
    Ext.Ajax.request({
        url: agenComboGet,
        //params: {
        //    p0: matUserID
        //},
        method: reqVal_p,
        success: function (response) {
            var data = Ext.decode(response.responseText);
            if (data.success) {
                var agen_nos = data.etts;
                if (agen_nos.length > 0) {
                    for (var i = 0; i < agen_nos.length; i++) {
                        agen_noQueryStore.add({ VALUE: agen_nos[i].VALUE, TEXT: agen_nos[i].TEXT });
                    }
                }
            }
        },
        failure: function (response, options) {

        }
    });
}
setComboData();

Ext.onReady(function () {


    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combobox',
                    fieldLabel: '報表類別',
                    name: 'ExportType',
                    queryMode: 'local',
                    displayField: 'name',
                    valueField: 'abbr',
                    width: 145,
                    labelWidth: 60,
                    editable: false,
                    store: [
                        { abbr: '', name: '不分' },
                        { abbr: 'Y', name: '零購' },
                        { abbr: 'N', name: '合約(99%)' }
                    ],
                    value: '',
                    padding: '0 4 0 4'
                }, {
                    xtype: 'monthfield',
                    fieldLabel: '年月份',
                    name: 'YYYMM',
                    labelWidth: 60,
                    width: 150,
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false,
                    value: new Date()
                }, {
                    xtype: 'combo',
                    fieldLabel: '排除廠商',
                    name: 'AGEN_NO',
                    enforceMaxLength: true,
                    labelWidth: 60,
                    width: 220,
                    padding: '0 4 0 4',
                    store: agen_noQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                },
                {
                    xtype: 'button',
                    text: '匯出檔案',
                    handler: function () {

                        if (T1Query.getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                            return;
                        }
                        var P0 = T1Query.getForm().findField('ExportType').getValue();
                        var P1 = T1Query.getForm().findField('YYYMM').rawValue;
                        var P2 = T1Query.getForm().findField('AGEN_NO').getValue();
                        var p = new Array();
                        p.push({ name: 'ExportType', value: P0 }); //SQL篩選條件
                        p.push({ name: 'YYYMM', value: P1 }); //SQL篩選條件
                        p.push({ name: 'AGEN_NO', value: P2 }); //SQL篩選條件
                        p.push({ name: 'path', value: '123' }); //SQL篩選條件
                        PostForm('/api/AA0085/Excel', p);
                        msglabel('訊息區:匯出檔案');
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        //f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
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
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            p.push({ name: 'P3', value: P3 }); //SQL篩選條件
                            p.push({ name: 'P4', value: matUserID }); //SQL篩選條件                            
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            },
            {
                id: 't1print', text: '列印', disabled: false, handler: function () {
                    showReport();
                }
            }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0070', { // 定義於/Scripts/app/store/AA/AA0092.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    P0: P0,
                    P1: P1,
                    P2: P2,
                    P3: P3,
                    P4: matUserID
                };
                Ext.apply(store.proxy.extraParams, np);

            },
        }
    });



    function T1Load() {

        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 160
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 80
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 100
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80
            }, {
                text: "調撥量",
                dataIndex: 'BW_MQTY',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "歸墊量",
                dataIndex: 'RV_MQTY',
                style: 'text-align:left',
                align: 'right',
                width: 60
            }, {
                text: "申請日",
                dataIndex: 'APPTIME',
                width: 60
            }, {
                text: "調撥日",
                dataIndex: 'DIS_TIME',
                width: 60
            }, {
                header: "",
                flex: 1
            }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                    T1Tool.down('#t1print').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        //resizeTabs: true,
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            }
        ]
    });


    T1Query.getForm().findField('YYYMM').focus();
});
