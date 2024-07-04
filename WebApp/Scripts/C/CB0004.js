Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app',
    }
});

Ext.require(['WEBAPP.utils.Common']);

var T1GetExcel = '/api/CB0004/DownloadExcel';

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //本地file
    var file;


    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 200;
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
                    xtype: 'filefield',
                    name: 'filefield',
                    fieldLabel: '上傳檔案',
                    buttonText: '選擇檔案',
                    width: 400,
                    padding :'0 20 0 0' ,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            var newValue = value.replace(/C:\\fakepath\\/g, '');
                            widget.setRawValue(newValue);
                            Ext.getCmp('check').setDisabled(true);
                            var files = event.target.files;
                            if (!files || files.length == 0) return; // make sure we got something
                            file = files[0];
                            var ext = this.value.split('.').pop();
                            if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                                Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                                T1Query.getForm().findField('filefield').fileInputEl.dom.value = '';
                                msglabel("訊息區:");
                            } else {
                                Ext.getCmp('check').setDisabled(false);
                                msglabel("已選擇檔案");
                            }
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '檢核',
                    id: 'check',
                    name: 'check',
                    disabled: true,
                    handler: function () {
                        Ext.getCmp('import').setDisabled(true);
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        var formData = new FormData();
                        formData.append("file", file);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/CB0004/CheckExcel",
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
                                            //Ext.getCmp('import').setDisabled(true);
                                            //IsSend = false;
                                        }
                                        else {
                                            msglabel("訊息區:檔案檢核成功");
                                            T1Store.loadData(data.etts, false);
                                            //IsSend = true;
                                            Ext.getCmp('import').setDisabled(false);
                                        }

                                        //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        myMask.hide();
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                    }
                                });
                    },
                },
                {
                    xtype: 'button',
                    text: '匯入',
                    id: 'import',
                    name: 'import',
                    disabled: true,
                    handler: function () {
                        Ext.getCmp('import').setDisabled(true);
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        msglabel("訊息區:匯入中");
                        Ext.Ajax.request({
                            url: '/api/CB0004/Import',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    Ext.MessageBox.alert("提示", "匯入完成");
                                    Ext.getCmp('import').setDisabled(true);
                                    T1Store.loadData(data.etts, false);
                                    msglabel("訊息區:匯入完成");
                                }
                                myMask.hide();
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
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    xtype: 'button',
                    text: '下載範本',
                    handler: function () {
                        //PostForm(T1GetExcel, null);
                        location.href = "../../Scripts/C/CB0004-上傳品項條碼資料範本.xls";
                        msglabel('訊息區:下載範本完成');
                    }
                }
            ]
        }]
    });


    var T1Store= Ext.create('Ext.data.Store', {
    });
    function T1Load() {
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
            xtype: 'rownumberer',
        }, {
            text: "原始資料",
            dataIndex: 'BARCODE_TEXT',
            width: 80,
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
        }, {
            text: "條碼",
            dataIndex: 'BARCODE',
            width: 140
        }, {
            text: "條碼類別代碼",
            dataIndex: 'XCATEGORY',
            width: 80
        }, {
            text: "條碼類別敘述",
            dataIndex: 'XCATEGORY_DISPLAY',
            width: 145
        }, {
            text: "檢核結果",
            dataIndex: 'STATUS_DISPLAY',
            width: 190
        }, {
            text: "異動結果",
            dataIndex: 'STATUS',
            width: 80
        }, {
            header: "",
            flex: 1
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

});
