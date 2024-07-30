Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    var reportUrl = '/Report/A/AB0122.aspx';　　　　　　　　//明細

    // 消耗條件
    var consumeClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { 'VALUE': '', 'TEXT': '全部' },
            { 'VALUE': '1', 'TEXT': '> 0' },
            { 'VALUE': '2', 'TEXT': '= 0' },
            { 'VALUE': '3', 'TEXT': '< 0' },
        ]
    });
    
    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
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
                    xtype: 'monthfield',
                    fieldLabel: '年月',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: mLabelWidth,
                    width: 140,
                    padding: '0 4 0 4',
                    value: new Date(),
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '排名前',
                    labelAlign: 'right',
                    name: 'P1',
                    labelWidth: 50,
                    width: 100,
                    maskRe: /[0-9]/,
                    fieldCls: '',
                    allowBlank: true
                }, {
                    xtype: 'label',
                    text: '名',
                    padding: '4 0 0 2',
                },
                //{
                //    fieldLabel: '顯示消耗為0的藥品',
                //    name: 'P2',
                //    xtype: 'checkboxfield',
                //    labelWidth: 140,
                //    width: 180,
                //    labelAlign: 'right',
                //    listeners: {
                //        change: function (checkbox, newValue, oldValue, eOpts) {

                //        }
                //    }
                //},
                {
                    xtype: 'combo',
                    store: consumeClassStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    fieldLabel: '消耗',
                    queryMode: 'local',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    lazyRender: true,
                    width: 122,
                    labelWidth: 62,
                    value: '',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (Ext.getCmp('P0').validate()) {
                            T1Load();
                        }
                        else {
                            Ext.Msg.alert('訊息', '查詢條件[年月]為必填');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        T1Query.getForm().findField('P0').reset();
                        T1Query.getForm().findField('P1').reset();

                        var f = this.up('form').getForm();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在D0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]

    });

    var T1Store = Ext.create('WEBAPP.store.AB.AB0122', {
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        msglabel('');
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 'print',
                name: 'print',
                text: '列印',
                handler: function () {
                    if (Ext.getCmp('P0').validate() && T1Query.getForm().findField('P1').getValue() != '') {
                        showReport();
                    }
                    else {
                        Ext.Msg.alert('訊息', '查詢條件[年月]及排名數為必填');
                    }
                }
            },

        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?DATA_YM=' + T1Query.getForm().findField('P0').rawValue
                    + '&ROWNUM=' + T1Query.getForm().findField('P1').getValue()
                    + '&NONE_COST=' + T1Query.getForm().findField('P2').getValue()
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        sortableColumns: false,
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
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
                text: "年月",
                dataIndex: 'DATA_YM',
                style: 'text-align:left',
                align: 'left',
                width: 60
            },
            {
                text: "健保碼",
                dataIndex: 'M_NHIKEY',
                style: 'text-align:left',
                align: 'left',
                width: 100
            }, {
                text: "藥品名稱",
                dataIndex: 'MMNAME_E',
                style: 'text-align:left',
                align: 'left',
                width: 150
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                style: 'text-align:left',
                align: 'left',
                width: 70
            }, {
                text: "進價",
                dataIndex: 'DISC_UPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                //text: "移動平均加權價",
                //dataIndex: 'AVG_PRICE',
                text: "優惠合約單價",
                dataIndex: 'DISC_CPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 130
            }, {
                text: "總量",
                dataIndex: 'MN_INQTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "總金額",
                dataIndex: 'TOT_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "結存量",
                dataIndex: 'INV_QTY',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "結存金額",
                dataIndex: 'FNL_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "期末比",
                dataIndex: 'RR',
                style: 'text-align:left',
                align: 'right',
                width: 80
            }, {
                text: "合約單價",
                dataIndex: 'CONT_PRICE',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "優惠百分比",
                dataIndex: 'RATIO',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "健保價",
                dataIndex: 'INSUAMOUNT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                style: 'text-align:left',
                width: 100
            }, {
                text: "消耗金額",
                dataIndex: 'CONSUME_AMT',
                style: 'text-align:left',
                align: 'right',
                width: 100
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
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
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });

    T1Query.getForm().findField('D0').focus();

});