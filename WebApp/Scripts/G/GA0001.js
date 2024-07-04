Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var statusComboGet = '/api/GA0001/StatusCombo';

    var windowWidth = $(window).width();
    var windowHeight = $(window).height();
    var status = "A";

    var T1LastRec = null;

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var viewModel = Ext.create('WEBAPP.store.GA0001VM');

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;

    // 狀態選單
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function getStatusCombo() {
        Ext.Ajax.request({
            url: statusComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var statuses = data.etts;
                    if (statuses.length > 0) {
                        statusStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < statuses.length; i++) {
                            statusStore.add({ VALUE: statuses[i].VALUE, TEXT: statuses[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Store = viewModel.getStore('TC_INVQMTR');
    function T1Load(clearMsg) {
        if (clearMsg) {
            msglabel('');
        }
        
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        //T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Tool.moveFirst();
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',

        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'numberfield',
                            fieldLabel: '庫存量天數<=',
                            labelAlign: 'right',
                            name: 'P0',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true,
                            width: 150,
                            labelSeparator: '',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '天',
                            labelSeparator: '',
                            width: 60
                        },
                        //{
                        //    xtype: 'combo',
                        //    fieldLabel: '訂購狀態',
                        //    labelAlign: 'right',
                        //    name: 'P1',   //TOWH
                        //    id: 'P1',
                        //    store: statusStore,
                        //    displayField: 'TEXT',
                        //    valueField: 'VALUE',
                        //    editable: false,
                        //    allowBlank: true,
                        //    anyMatch: true,
                        //    //typeAhead: true,
                        //    //forceSelection: true,
                        //    width: 220,
                        //    queryMode: 'local',
                        //    triggerAction: 'all',
                        //},
                        //{
                        //    xtype: 'radiogroup',
                        //    name: 'P1',
                        //    fieldLabel: '訂購狀態',
                        //    labelAlign: 'right',
                        //    items: [
                        //        { boxLabel: '未訂購', name: 'P1', inputValue: 'A', width: 70, checked: true },
                        //        { boxLabel: '已訂購待確認', name: 'P1', inputValue: 'B', width: 100 }
                        //    ],
                        //    listeners: {
                        //        change: function (field, newValue, oldValue) {
                        //            changeStatus(newValue['P1']);
                        //        }
                        //    },
                        //    labelWidth: 70
                        //},
                        {
                            xtype: 'textfield',
                            fieldLabel: '藥品名稱',
                            name: 'P2',
                            id: 'P2',
                            labelAlign: 'right',
                            enforceMaxLength: false,
                            maxLength: 100,
                            labelWidth:60,
                            width: 160,
                            padding: '0 4 0 4'
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {

                                T1Load(true);

                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                this.up('form').getForm().reset();

                            }
                        }
                    ]
                }
            ]
        }]
    });
    var changeStatus = function (value) {
        status = value;
        if (status == "A") {
            Ext.getCmp('makeOrder').enable();
            Ext.getCmp('cancelOrder').disable();
        } else {
            Ext.getCmp('makeOrder').disable();
            Ext.getCmp('cancelOrder').enable();
        }
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '計算平均月消耗量',
                id: 'calc',
                name: 'calc',
                handler: function () {
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/GA0001/Calculate',
                        method: reqVal_p,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            
                            myMask.hide();
                            if (data.success) {
                                msglabel('平均月消耗量計算完成');
                            }
                        },
                        failure: function (response, options) {
                            myMask.hide();
                            Ext.MessageBox.alert('提示', '發生例外錯誤');
                        }
                    });
                }
            },
            {
                xtype: 'filefield',
                name: 'import',
                id: 'import',
                buttonText: '匯入庫存量',
                buttonOnly: true,
                padding: '0 4 0 0',
                width: 72,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        var file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            Ext.getCmp('import').fileInputEl.dom.value = '';
                            msglabel('');
                        } else {
                            msglabel("已選擇檔案");
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", file);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/GA0001/Import",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    myMask.hide();
                                    if (!data.success) {
                                        T1Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('import').fileInputEl.dom.value = '';
                                        //Ext.getCmp('import').setDisabled(true);
                                        //IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:資料匯入成功");
                                        Ext.getCmp('import').fileInputEl.dom.value = '';

                                        T1Load();
                                    }

                                    //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    myMask.hide();
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('import').setRawValue("");
                                }
                            });
                        }
                    }

                }

            },
            //{
            //    text: '訂購',
            //    id: 'makeOrder',
            //    name: 'makeOrder',
            //    handler: function () {
            //        var tempData = T1Grid.getSelection();
            //        if (tempData.length == 0) {
            //            Ext.Msg.alert('提示', '請選擇項目');
            //            return;
            //        }

            //        setOrder(tempData, 'B');
            //    }
            //},
            //{
            //    text: '取消訂購',
            //    id: 'cancelOrder',
            //    name: 'cancelOrder',
            //    handler: function () {
            //        var tempData = T1Grid.getSelection();

            //        if (tempData.length == 0) {
            //            Ext.Msg.alert('提示', '請選擇項目');
            //            return;
            //        }
            //        setOrder(tempData, 'A');
            //    }
            //},
            //{
            //    text: '下載範例',
            //    id: 'example',
            //    name: 'example',
            //    handler: function () {
            //        location.href = "../../Scripts/G/GA0001-中藥訂購作業匯入範本.xls";
            //        msglabel('訊息區:下載範本完成');
            //    }
            //}
            {
                xtype: 'docbutton',
                name: 'example',
                id: 'example',
                text: '下載範例',
                documentKey: 'GA0001'
            }
        ]
    });

    function setOrder(selection, status) {
        
        var data = [];

        for (var i = 0; i < selection.length; i++) {
            selection[i].data.PURCH_ST = status;
            var item = {
                DATA_YM: selection[i].data.DATA_YM,
                MMCODE: selection[i].data.MMCODE,
                PURCH_ST: status
            }
            data.push(item);
        }

        Ext.Ajax.request({
            url: '/api/GA0001/Set',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(data) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('狀態更新成功');
                    T1Load(false);
                } else {
                    Ext.Msg.alert('錯誤', '發生例外事件');
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('錯誤', '發生例外事件');
            }
        });
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "資料年月",
                dataIndex: 'DATA_YM',
                width: 80
            },
            {
                text: "電腦編號",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "藥品名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70
            },
            {
                text: "進價",
                dataIndex: 'IN_PRICE',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "上月庫存",
                dataIndex: 'PMN_INVQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "本月進貨",
                dataIndex: 'MN_INQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "本月消耗",
                dataIndex: 'MN_USEQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "本月庫存",
                dataIndex: 'MN_INVQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "存放位置",
                dataIndex: 'STORE_LOC',
                width: 120
            },
            {
                text: "前6個月平均消耗量",
                dataIndex: 'M6AVG_USEQTY',
                width: 130,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "前3個月平均消耗量",
                dataIndex: 'M3AVG_USEQTY',
                width: 130,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "前6個月最大月消耗量",
                dataIndex: 'M6MAX_USEQTY',
                width: 140,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "前3個月最大月消耗量",
                dataIndex: 'M3MAX_USEQTY',
                width: 140,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "庫存量天數",
                dataIndex: 'INV_DAY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "應訂購量",
                dataIndex: 'EXP_PURQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "藥商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 90
            },
            {
                text: "單位劑量",
                dataIndex: 'PUR_UNIT',
                width: 90
            },
            {
                text: "進貨單價",
                dataIndex: 'IN_PURPRICE',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "計量單位乘數",
                dataIndex: 'BASEUN_MULTI',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單位劑量乘數",
                dataIndex: 'PURUN_MULTI',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "建議訂購量",
                dataIndex: 'RCM_PURQTY',
                width: 90,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "建議訂購天數",
                dataIndex: 'RCM_PURDAY',
                width: 100,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "建立時間",
                dataIndex: 'CREATE_TIME',
                width: 130
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;

            }
        }
    });

    // --------------------

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
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
                        split: true,
                        items: [T1Grid]
                    },

                ]
            }
        ]
    });


    var myMask = new Ext.LoadMask(viewport, { msg: '處理中' });
    getStatusCombo();

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        //newApplyWindow.setHeight(windowHeight);
        //T31Grid.setHeight(windowHeight / 2 - 30);
        //T32Grid.setHeight(windowHeight / 2 - 30);

        //distributionWindow.setHeight(windowHeight);
        //T51Grid.setHeight(windowHeight / 2 - 30);
        //T52Grid.setHeight(windowHeight / 2 - 30);

        //distritedListnWindow.setHeight(windowHeight);
        //T61Grid.setHeight(windowHeight / 2 - 30);
        //T62Grid.setHeight(windowHeight / 2 - 30);

    });

});