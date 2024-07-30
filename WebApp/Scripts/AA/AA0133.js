Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var reportUrl = '/Report/A/AA0133.aspx';

    var T1RecLength = 0;
    var T1LastRec = null;

    var getWh = '../../../api/AA0133/GetWH';
    var T1GetExcel = '../../../api/AA0133/Excel';

    var userId = session['UserId'];

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAll = "N";
    if (Ext.getUrlParam('isAll') != null) {
        isAll = JSON.parse(Ext.getUrlParam('isAll'));
    }

    var storeWhNo = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
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
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'combo',
                        store: storeWhNo,
                        fieldLabel: '庫房代碼',
                        name: 'P0',
                        id: 'P0',
                        width: 200,
                        padding: '0 4 0 4',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        displayField: 'WH_NO',
                        valueField: 'WH_NO',
                        fieldCls: 'required',
                        allowBlank: false,
                        tpl: new Ext.XTemplate(
                            '<tpl for=".">',
                            '<tpl if="VALUE==\'\'">',
                            '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                            '<tpl else>',
                            '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                            '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                            '</tpl></tpl>', {
                                formatText: function (text) {
                                    return Ext.util.Format.htmlEncode(text);
                                }
                            }),
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            }
                        }
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
                                var index2 = getColumnIndex(columns, 'STORE_QTY_TIME');
                                
                                T1Grid.getColumns()[index1].hide();
                                T1Grid.getColumns()[index2].hide();

                                if (newValue == true) {

                                    T1Grid.getColumns()[index1].show();
                                    T1Grid.getColumns()[index2].show();
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
                            if (!f.findField('P0').getValue()) {
                                Ext.MessageBox.alert('提示', '<span style="color:red">庫房代碼</span>為必填');
                                return;
                            }
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
            }]
    });

    function setComboData() {
        //庫房代碼
        Ext.Ajax.request({
            url: getWh,
            params: {
                isAll: isAll
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P0 = T1Query.getForm().findField('P0');

                    if (tb_wh_no.length === 1) {
                        storeWhNo.add({ WH_NO: tb_wh_no[0].WH_NO, WH_NAME: tb_wh_no[0].WH_NAME });
                        combo_P0.setValue(tb_wh_no[0].WH_NO);
                    }
                    else {
                        combo_P0.setDisabled(false);

                        if (tb_wh_no.length > 0) {
                            for (var i = 0; i < tb_wh_no.length; i++) {
                                storeWhNo.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });
                            }
                        }
                    }
                }
            }
        });
    }
    
    setComboData();
    Ext.define('AA0133_Model', {
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
        model: 'AA0133_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0133/All',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
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
                    var p0 = T1Query.getForm().findField('P0').getValue();
                    var p1 = T1Query.getForm().findField('P1').getValue();
                    var p2 = T1Query.getForm().findField('P2').getValue() == true ? 'Y' : 'N';
                    var p = new Array();

                    p.push({ name: 'P0', value: p0 });
                    p.push({ name: 'P1', value: p1 });
                    p.push({ name: 'P2', value: p2 });
                    PostForm(T1GetExcel, p);
                }
            },
            {
                itemId: 'print',
                id: 'print',
                name: 'print',
                text: '列印',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport();
                }
            },
            {
                itemId: 'print2',
                id: 'print2',
                name: 'print',
                text: '印出全部',
                style: 'margin:0px 5px;',
                handler: function () {
                    showReport2();
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
            text: "盤點電腦量",
            dataIndex: 'STORE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 90,
            hidden:true
        }, {
            text: "盤點時間",
            dataIndex: 'STORE_QTY_TIME',
            width: 120,
            hidden: true
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
            }
        }
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
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + userId + 
                      '&isAll=false" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
                html: '<iframe src="' + reportUrl + '?p0=null&p1=' + p1 + '&p2=' + userId + '&isAll=' + isAll
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