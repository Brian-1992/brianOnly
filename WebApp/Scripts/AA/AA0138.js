Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var reportUrl1 = '/Report/A/AA0138.aspx';
    var reportUrl2 = '/Report/A/AA0138_WH.aspx';

    var T1RecLength = 0;
    var T1LastRec = null;
    var windowHeight = $(window).height();

    var T1GetExcel = '../../../api/AA0138/Excel';
    var T2GetExcel = '../../../api/AA0138/WhExcel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var storeDrugType = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [     // 若需修改條件，除修改此處也需修改CB0006Repository中的status條件
            { "VALUE": "3", "TEXT": "1-3級管制藥品" },
            { "VALUE": "4", "TEXT": "4級管制藥品" }
        ]
    });
    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: 'hbox',
                items: [

                    {
                        xtype: 'panel',
                        id: 'PanelP1',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'displayfield',
                                fieldLabel: '庫房代碼',
                                value: '全院三級庫'
                            },
                            {
                                xtype: 'combo',
                                store: storeDrugType,
                                fieldLabel: '管制藥分類',
                                name: 'P1',
                                id: 'P1',
                                //width: 120,
                                padding: '0 4 0 4',
                                queryMode: 'local',
                                anyMatch: true,
                                autoSelect: true,
                                displayField: 'TEXT',
                                valueField: 'VALUE',
                                fieldCls: 'required',
                                allowBlank: false,
                            },
                            {
                                xtype: 'checkboxfield',
                                boxLabel: '顯示盤點電腦量',
                                name: 'P2',
                                id: 'P2',
                                style: 'margin:0px 5px 0px 50px;',
                                labelWidth: 80,
                                width: 120,
                                listeners: {
                                    change: function (self, newValue, oldValue, eOpts) {
                                        var columns = T1Grid.getColumns();
                                        var index1 = getColumnIndex(columns, 'STORE_QTY');

                                        T1Grid.getColumns()[index1].hide();
                                        if (newValue == true) {
                                            T1Grid.getColumns()[index1].show();
                                        }
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                style: 'margin:0px 5px 0px 20px;',
                                handler: function () {
                                    var f = T1Query.getForm();
                                    if (!f.findField('P1').getValue()) {
                                        Ext.MessageBox.alert('提示', '<span style="color:red">管制藥類別</span>為必填');
                                        return;
                                    }
                                    T1Load();
                                }
                            }, {
                                xtype: 'button',
                                text: '清除',
                                style: 'margin:0px 5px;',
                                handler: function () {
                                    var f = this.up('form').getForm();
                                    f.reset();
                                    f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                    msglabel('訊息區:');
                                }
                            }
                        ]
                    }
                ]
            }, {
                xtype: 'component',
                flex: 1
            },
            {
                xtype: 'container',
                layout: 'hbox',
                right: '100%',
                style: 'margin:0px 5px;',
                items: [
                    {
                        xtype: 'button',
                        text: '查詢各庫資料',
                        style: 'margin:0px 5px;',
                        handler: function () {
                            parent.link2('/Form/Index/AA0133?isAll="Y"', '三級庫管制藥品清點查詢(AA0133)', true);
                            //parent.link2('/Form/Index/AA0133', ' 初盤數量輸入作業(CE0016)', true);
                        }
                    }
                ]
            }]
    });

    Ext.define('AA0138_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'PMN_INVQTY', type: 'string' },

            { name: 'MN_INQTY', type: 'string' },
            { name: 'USE_QTY', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'CHK_QTY', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'AA0138_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0138/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                    Ext.getCmp('print').setDisabled(false);
                } else {
                    Ext.getCmp('export').setDisabled(true);
                    Ext.getCmp('print').setDisabled(true);
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                id: 'export',
                name: 'export',
                handler: function () {
                    var p1 = T1Query.getForm().findField('P1').getValue();
                    var p2 = T1Query.getForm().findField('P2').getValue() == true ? 'Y' : 'N';
                    var p = new Array();

                    //p.push({ name: 'P0', value: p0 });
                    p.push({ name: 'P1', value: p1 });
                    p.push({ name: 'P2', value: p2 });
                    PostForm(T1GetExcel, p);
                }
            },
            {
                itemId: 'print',
                id: 'print',
                name: 'print',
                text: '列印總表',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport();
                }
            },
            {
                itemId: 'print2',
                id: 'print2',
                name: 'print',
                text: '庫房盤點現況查詢',
                style: 'margin:0px 5px;',
                handler: function () {
                    //showReport2();
                    T2Load();
                    whWindow.show();
                }
            }

        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            style: 'text-align:left',
            align: 'left',
            width: 210
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            style: 'text-align:left',
            align: 'left',
            width: 180
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            style: 'text-align:left',
            align: 'left',
            width: 65
        }, {
            text: "上月結存",
            dataIndex: 'PMN_INVQTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "本月進貨",
            dataIndex: 'MN_INQTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "本月消耗",
            dataIndex: 'USE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "本月結存",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: " 實際清點",
            dataIndex: 'CHK_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: " 期初至盤點時間<br>醫令扣庫數量",
            dataIndex: 'ORDER_TOTAL',
            style: 'text-align:left',
            align: 'right',
            width: 110
        }, {
            text: "盤點總電腦量",
            dataIndex: 'STORE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100,
            hidden: true
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
    });

    Ext.define('AA0138_WH_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'MMCODE_CNT', type: 'string' },
            { name: 'IS_CHK_BATCH', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'AA0138_WH_Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0138/WhData',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                //var dataCount = store.getCount();
                //if (dataCount > 0) {
                //    Ext.getCmp('wh_export').setDisabled(false);
                //    Ext.getCmp('wh_print').setDisabled(false);
                //} else {
                //    Ext.getCmp('wh_export').setDisabled(true);
                //    Ext.getCmp('wh_print').setDisabled(true);
                //}
            }
        }
    });

    function T2Load() {
        T2Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'wh_export',
                text: '匯出',
                id: 'wh_export',
                name: 'wh_export',
                handler: function () {
                    var p1 = T1Query.getForm().findField('P1').getValue();

                    var p = new Array();

                    p.push({ name: 'P1', value: p1 });

                    PostForm(T2GetExcel, p);
                }
            },
            {
                itemId: 'wh_print',
                id: 'wh_print',
                name: 'wh_print',
                text: '列印',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport2();
                }
            }
        ]
    });

    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        height: windowHeight,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
        columns: [{
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "清點總數",
            dataIndex: 'MMCODE_CNT',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "是否開單",
            dataIndex: 'IS_CHK_BATCH',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }]
    });

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    function showReport() {
        if (!win) {

            var np = {
                p1: T1Query.getForm().findField('P1').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl1 + '?p1=' + np.p1 +
                '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    function showReport2() {
        var p1 = T1Query.getForm().findField('P1').getValue();
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl2 + '?p0=null&p1=' + p1
                + '&isAll=true" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    var whWindow = Ext.create('Ext.window.Window', {
        id: 'whWindow',
        renderTo: Ext.getBody(),
        items: [T2Grid],
        modal: true,
        width: "500px",
        height: windowHeight,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "庫房盤點現況",
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    whWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                whWindow.center();
                whWindow.setWidth(500);
                whWindow.setHeight(windowHeight);
            }
        }
    });
    whWindow.hide();

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
    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('print').setDisabled(true);
    T1Query.getForm().findField('P1').setValue('3');
});